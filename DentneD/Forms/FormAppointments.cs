﻿#region License
// Copyright (c) 2015 Davide Gironi
//
// Please refer to LICENSE file for licensing information.
#endregion

using System;
using System.Linq;
using System.IO;
using DG.UI.GHF;
using DG.DentneD.Model;
using DentneD;
using DG.DentneD.Model.Entity;
using System.Configuration;
using System.Windows.Forms.Calendar;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data;

namespace DG.DentneD.Forms
{
    public partial class FormAppointments : DGUIGHFForm
    {
        private DentneDModel _dentnedModel = null;

        private TabElement tabElement_tabAppointments = new TabElement();

        private readonly int calendarDayHourBegin = 9;
        private readonly int calendarDayHourEnd = 19;

        private DateTime _currentDate = DateTime.Now;
        private List<CustomAppointmentItem> _appointmentItems = new List<CustomAppointmentItem>();
        private EditingMode _currentEditingMode = EditingMode.R;
        private bool _calendarDoubleClick = false;
        private int _selectedAppointmentId = -1;
        private DayOfWeek _calendarFirstDayOfTheWeek = DayOfWeek.Monday;

        private const int CalendarTitleDayMaxLengh = 100;
        private const int CalendarTitleWeekMaxLengh = 50;
        private const int CalendarTitleMonthMaxLengh = 20;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormAppointments()
        {
            InitializeComponent();

            Initialize(Program.uighfApplication);

            _dentnedModel = new DentneDModel();

            calendarDayHourBegin = Convert.ToInt16(ConfigurationManager.AppSettings["calendarDayHourBegin"]);
            calendarDayHourEnd = Convert.ToInt16(ConfigurationManager.AppSettings["calendarDayHourEnd"]);
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                if(dayOfWeek.ToString().CompareTo(ConfigurationManager.AppSettings["calendarFirstDayOfTheWeek"]) == 0)
                {
                    _calendarFirstDayOfTheWeek = dayOfWeek;
                    break;
                }
            }
            bool calendarWeekIncludeSaturday = Convert.ToBoolean(ConfigurationManager.AppSettings["calendarWeekIncludeSaturday"]);
            bool calendarWeekIncludeSunday = Convert.ToBoolean(ConfigurationManager.AppSettings["calendarWeekIncludeSunday"]);

            CalendarHighlightRange highlightrangeMonday = new CalendarHighlightRange();
            highlightrangeMonday.DayOfWeek = DayOfWeek.Monday;
            highlightrangeMonday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeMonday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeTuesday = new CalendarHighlightRange();
            highlightrangeTuesday.DayOfWeek = DayOfWeek.Tuesday;
            highlightrangeTuesday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeTuesday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeWednesday = new CalendarHighlightRange();
            highlightrangeWednesday.DayOfWeek = DayOfWeek.Wednesday;
            highlightrangeWednesday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeWednesday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeThursday = new CalendarHighlightRange();
            highlightrangeThursday.DayOfWeek = DayOfWeek.Thursday;
            highlightrangeThursday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeThursday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeFriday = new CalendarHighlightRange();
            highlightrangeFriday.DayOfWeek = DayOfWeek.Friday;
            highlightrangeFriday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeFriday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeSaturday = new CalendarHighlightRange();
            highlightrangeSaturday.DayOfWeek = DayOfWeek.Saturday;
            highlightrangeSaturday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeSaturday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");
            CalendarHighlightRange highlightrangeSunday = new CalendarHighlightRange();
            highlightrangeSunday.DayOfWeek = DayOfWeek.Sunday;
            highlightrangeSunday.StartTime = TimeSpan.Parse(calendarDayHourBegin + ":00:00");
            highlightrangeSunday.EndTime = TimeSpan.Parse((calendarDayHourEnd + 1) + ":00:00");

            CalendarHighlightRange[] highlightrange = { highlightrangeMonday, highlightrangeTuesday, highlightrangeWednesday, highlightrangeThursday, highlightrangeFriday };
            if (calendarWeekIncludeSaturday)
                highlightrange = highlightrange.Concat(new CalendarHighlightRange[] { highlightrangeSaturday }).ToArray();
            if (calendarWeekIncludeSunday)
                highlightrange = highlightrange.Concat(new CalendarHighlightRange[] { highlightrangeSunday }).ToArray();

            calendar_listdays.HighlightRanges = highlightrange;
            calendar_listdays.TimeScale = CalendarTimeScale.ThirtyMinutes;
            calendar_listdays.TimeUnitsOffset = -calendarDayHourBegin * (60 / (int)calendar_listdays.TimeScale); // *23 + 58;
            calendar_listdays.AllowDrop = false;
            calendar_listdays.AllowItemEdit = false;
            calendar_listdays.AllowItemResize = false;
            calendar_listdays.AllowNew = false;

            calendar_listweeks.HighlightRanges = highlightrange;
            calendar_listweeks.TimeScale = CalendarTimeScale.ThirtyMinutes;
            calendar_listweeks.TimeUnitsOffset = -calendarDayHourBegin * (60 / (int)calendar_listweeks.TimeScale);
            calendar_listweeks.AllowDrop = false;
            calendar_listweeks.AllowItemEdit = false;
            calendar_listweeks.AllowItemResize = false;
            calendar_listweeks.AllowNew = false;
            //calendar_listweeks.Height = 24 * (60 / (int)calendar_listweeks.TimeScale) * 23 + 58;

            ////            int scrollvalue = 0;
            //scrollvalue = calendarDayHourBegin * (60 / (int)calendar_listdays.TimeScale) * 23 + 58 - 30;
            ////if (scrollvalue > tabPage_tabListDays.VerticalScroll.Minimum && scrollvalue < tabPage_tabListDays.VerticalScroll.Maximum)
            ////    tabPage_tabListDays.AutoScrollPosition = new Point(0, scrollvalue);
            //scrollvalue = calendarDayHourBegin * (60 / (int)calendar_listmonths.TimeScale) * 23 + 58 - 30;
            //if (scrollvalue > tabPage_tabListWeeks.VerticalScroll.Minimum && scrollvalue < tabPage_tabListWeeks.VerticalScroll.Maximum)
            //    tabPage_tabListWeeks.AutoScrollPosition = new Point(0, scrollvalue);

            calendar_listmonths.HighlightRanges = highlightrange;
            calendar_listmonths.TimeScale = CalendarTimeScale.ThirtyMinutes;
            calendar_listmonths.TimeUnitsOffset = -calendarDayHourBegin * (30 / (int)calendar_listmonths.TimeScale);
            calendar_listmonths.AllowDrop = false;
            calendar_listmonths.AllowItemEdit = false;
            calendar_listmonths.AllowItemResize = false;
            calendar_listmonths.AllowNew = false;
            calendar_listmonths.MaximumViewDays = 42;

            IsBindingSourceLoading = true;
            _selectedAppointmentId = -1;
            appointmentsBindingSource.DataSource = new appointments();
            IsBindingSourceLoading = false;
        }

        /// <summary>
        /// Loader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAppointments_Load(object sender, EventArgs e)
        {
            IsBindingSourceLoading = true;
            PreloadView();
            if (comboBox_filterRooms.Items.Count == 2)
                comboBox_filterRooms.SelectedIndex = 1;
            if (comboBox_filterDoctors.Items.Count == 2)
                comboBox_filterDoctors.SelectedIndex = 1;
            IsBindingSourceLoading = false;

            _currentEditingMode = EditingMode.R;
            SetCustomEditingMode(false);
            
            LoadAppointments();
        }

        /// <summary>
        /// Preload View
        /// </summary>
        private void PreloadView()
        {
            //load doctors
            doctors_idComboBox.DataSource = _dentnedModel.Doctors.List().Select(r => new { name = r.doctors_surname + " " + r.doctors_name, r.doctors_id }).OrderBy(r => r.name).ToList();
            doctors_idComboBox.DisplayMember = "name";
            doctors_idComboBox.ValueMember = "doctors_id";

            //load rooms
            rooms_idComboBox.DataSource = _dentnedModel.Rooms.List().OrderBy(r => r.rooms_name).ToList();
            rooms_idComboBox.DisplayMember = "rooms_name";
            rooms_idComboBox.ValueMember = "rooms_id";

            //load patients
            patients_idComboBox.DataSource = _dentnedModel.Patients.List().Select(r => new { name = r.patients_surname + " " + r.patients_name, r.patients_id }).OrderBy(r => r.name).ToList();
            patients_idComboBox.DisplayMember = "name";
            patients_idComboBox.ValueMember = "patients_id";

            //load filter doctors
            comboBox_filterDoctors.Items.Clear();
            comboBox_filterDoctors.Items.Add(new DGUIGHFUtilsUI.DGComboBoxItem("-1", ""));
            foreach (doctors a in _dentnedModel.Doctors.List().OrderBy(r => r.doctors_surname))
            {
                comboBox_filterDoctors.Items.Add(new DGUIGHFUtilsUI.DGComboBoxItem(a.doctors_id.ToString(), a.doctors_surname + " " + a.doctors_name));
            }
            comboBox_filterDoctors.SelectedIndex = -1;

            //load filter rooms
            comboBox_filterRooms.Items.Clear();
            comboBox_filterRooms.Items.Add(new DGUIGHFUtilsUI.DGComboBoxItem("-1", ""));
            foreach (rooms a in _dentnedModel.Rooms.List().OrderBy(r => r.rooms_name))
            {
                comboBox_filterRooms.Items.Add(new DGUIGHFUtilsUI.DGComboBoxItem(a.rooms_id.ToString(), a.rooms_name));
            }
            comboBox_filterRooms.SelectedIndex = -1;
        }

        
        #region filters

        /// <summary>
        /// Day filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthView_filterDay_DateChanged(object sender, System.Windows.Forms.DateRangeEventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            if (_currentDate != new DateTime(monthView_filterDay.SelectionStart.Year, monthView_filterDay.SelectionStart.Month, 1))
            {
                LoadAppointments();
            }
            else
            {
                ReloadCalendarItems();
            }
        }

        /// <summary>
        /// Doctor filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_filterDoctors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            LoadAppointments();
        }

        /// <summary>
        /// Room filter changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_filterRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            LoadAppointments();
        }

        /// <summary>
        /// Treatments advices filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_filterTreatmentsadvices_CheckedChanged(object sender, EventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            LoadAppointments();
        }

        #endregion


        #region Calendar list

        /// <summary>
        /// Reload all appointements
        /// </summary>
        private void LoadAppointments()
        {
            _currentDate = new DateTime(monthView_filterDay.SelectionStart.Year, monthView_filterDay.SelectionStart.Month, 1);

            _appointmentItems.Clear();
            
            List<appointments> appointments = new List<appointments>();

            DateTime fromdate = new DateTime(monthView_filterDay.SelectionStart.Year, monthView_filterDay.SelectionStart.Month, 1).AddDays(-7);
            DateTime todate = new DateTime(monthView_filterDay.SelectionStart.Year, monthView_filterDay.SelectionStart.Month, 1).AddMonths(1).AddDays(7).AddSeconds(-1);

            //load appointements
            if (comboBox_filterDoctors.SelectedIndex != -1 && comboBox_filterDoctors.SelectedIndex != 0)
            {
                int doctors_id = Convert.ToInt32(((DGUIGHFUtilsUI.DGComboBoxItem)comboBox_filterDoctors.SelectedItem).Id);
                if (comboBox_filterRooms.SelectedIndex != -1 && comboBox_filterRooms.SelectedIndex != 0)
                {
                    int rooms_id = Convert.ToInt32(((DGUIGHFUtilsUI.DGComboBoxItem)comboBox_filterRooms.SelectedItem).Id);
                    appointments = _dentnedModel.Appointments.List(r =>
                            r.appointments_from >= fromdate &&
                            r.appointments_from <= todate &&
                            r.doctors_id == doctors_id &&
                            r.rooms_id == rooms_id).ToList();
                }
                else
                {
                    appointments = _dentnedModel.Appointments.List(r =>
                            r.appointments_from >= fromdate &&
                            r.appointments_from <= todate &&
                            r.doctors_id == doctors_id).ToList();
                }                
            }

            foreach (appointments appointment in appointments)
            {
                patients patient = _dentnedModel.Patients.Find(appointment.patients_id);
                string titleday = patient.patients_surname + " " + patient.patients_name + " (" + appointment.appointments_title + ")";
                titleday = (titleday.Length > CalendarTitleDayMaxLengh ? titleday.Substring(0, CalendarTitleDayMaxLengh) + "..." : titleday);
                string titleweek = patient.patients_surname + " " + patient.patients_name;
                titleweek = (titleweek.Length > CalendarTitleWeekMaxLengh ? titleday.Substring(0, CalendarTitleWeekMaxLengh) + "..." : titleweek);
                string titlemonth = (patient.patients_surname.Length > 3 ? patient.patients_surname.Substring(0, 3) + "." : patient.patients_surname) + " " + patient.patients_surname.Substring(0, 1) + "."; 
                titlemonth = (titlemonth.Length > CalendarTitleMonthMaxLengh ? titlemonth.Substring(0, CalendarTitleMonthMaxLengh) + "..." : titlemonth);

                _appointmentItems.Add(new CustomAppointmentItem()
                {
                    DateFrom = appointment.appointments_from,
                    DateTo = appointment.appointments_to,
                    TitleDay = titleday,
                    TitleWeek = titleweek,
                    TitleMonth = titlemonth,
                    Color = getCalendarItemColor(appointment.rooms_id),
                    AppointmentId = appointment.appointments_id,
                    PatientTreatmentId = -1
                });
            }

            if (checkBox_filterTreatmentsadvices.Checked)
            {
                foreach (patientstreatments patientstreatment in _dentnedModel.PatientsTreatments.List(r =>
                    r.patientstreatments_expirationdate != null &&
                    r.patientstreatments_expirationdate >= fromdate &&
                    r.patientstreatments_expirationdate <= todate))
                {
                    patients patient = _dentnedModel.Patients.Find(patientstreatment.patients_id);
                    treatments treatment = _dentnedModel.Treatments.Find(patientstreatment.treatments_id);
                    string titleday = "ADV " + treatment.treatments_code + " - " + patient.patients_surname + " " + patient.patients_name;
                    titleday = (titleday.Length > CalendarTitleDayMaxLengh ? titleday.Substring(0, CalendarTitleDayMaxLengh) + "..." : titleday);
                    string titleweek = "ADV " + treatment.treatments_code + " - " + patient.patients_surname + " " + patient.patients_name;
                    titleweek = (titleweek.Length > CalendarTitleWeekMaxLengh ? titleday.Substring(0, CalendarTitleWeekMaxLengh) + "..." : titleweek);
                    string titlemonth = "ADV " + treatment.treatments_code + " - " + (patient.patients_surname.Length > 3 ? patient.patients_surname.Substring(0, 3) + "." : patient.patients_surname) + " " + patient.patients_surname.Substring(0, 1) + ".";
                    titlemonth = (titlemonth.Length > CalendarTitleMonthMaxLengh ? titlemonth.Substring(0, CalendarTitleMonthMaxLengh) + "..." : titlemonth);

                    _appointmentItems.Add(new CustomAppointmentItem()
                    {
                        DateFrom = patientstreatment.patientstreatments_expirationdate.Value.AddHours(12),
                        DateTo = patientstreatment.patientstreatments_expirationdate.Value.AddHours(13),
                        TitleDay = titleday,
                        TitleWeek = titleweek,
                        TitleMonth = titlemonth,
                        Color = getCalendarItemColor(-1),
                        AppointmentId = -1,
                        PatientTreatmentId = patientstreatment.patientstreatments_id
                    });
                }
            }

            ReloadCalendarItems();
        }

        /// <summary>
        /// Reload calendar items
        /// </summary>
        private void ReloadCalendarItems()
        {
            IsBindingSourceLoading = true;
            _selectedAppointmentId = -1;
            appointmentsBindingSource.DataSource = new appointments();
            IsBindingSourceLoading = false;

            if (tabControl_list.SelectedTab == tabPage_tabListDays)
            {
                calendar_listdays.SetViewRange(monthView_filterDay.SelectionStart, monthView_filterDay.SelectionStart);

                calendar_listdays.Items.Clear();

                foreach (CustomAppointmentItem item in _appointmentItems) { 
                    CustomCalendarItem cal = new CustomCalendarItem(calendar_listdays,
                        item.DateFrom,
                        item.DateTo,
                        item.TitleDay);
                    cal.AppointmentId = item.AppointmentId;
                    cal.PatientTreatmentId = -1;
                    cal.ApplyColor(item.Color);

                    if (calendar_listdays.ViewIntersects(cal))
                        calendar_listdays.Items.Add(cal);
                }

                calendar_listdays.Invalidate();
            }
            else if (tabControl_list.SelectedTab == tabPage_tabListWeeks)
            {
                int delta = _calendarFirstDayOfTheWeek - monthView_filterDay.SelectionStart.DayOfWeek;
                if (delta > 0)
                    delta -= 7;
                DateTime firstDayInWeek = monthView_filterDay.SelectionStart.AddDays(delta);
                calendar_listweeks.SetViewRange(firstDayInWeek, firstDayInWeek.AddDays(6));

                calendar_listweeks.Items.Clear();

                foreach (CustomAppointmentItem item in _appointmentItems)
                {
                    CustomCalendarItem cal = new CustomCalendarItem(calendar_listweeks,
                        item.DateFrom,
                        item.DateTo,
                        item.TitleWeek);
                    cal.AppointmentId = item.AppointmentId;
                    cal.PatientTreatmentId = -1;
                    cal.ApplyColor(item.Color);

                    if (calendar_listweeks.ViewIntersects(cal))
                        calendar_listweeks.Items.Add(cal);
                }

                calendar_listweeks.Invalidate();
            }
            else if (tabControl_list.SelectedTab == tabPage_tabListMonths)
            {
                DateTime firstDayOfTheMonth = new DateTime(monthView_filterDay.SelectionStart.Year, monthView_filterDay.SelectionStart.Month, 1);
                int delta = _calendarFirstDayOfTheWeek - firstDayOfTheMonth.DayOfWeek;
                if (delta > 0)
                    delta -= 7;
                DateTime firstDayInWeekMonth = firstDayOfTheMonth.AddDays(delta);
                calendar_listmonths.SetViewRange(firstDayInWeekMonth, firstDayInWeekMonth.AddMonths(1).AddDays(6));
                calendar_listmonths.Items.Clear();

                foreach (CustomAppointmentItem item in _appointmentItems)
                {
                    CustomCalendarItem cal = new CustomCalendarItem(calendar_listmonths,
                        item.DateFrom,
                        item.DateTo,
                        item.TitleMonth);
                    cal.AppointmentId = item.AppointmentId;
                    cal.PatientTreatmentId = -1;
                    cal.ApplyColor(item.Color);

                    if (calendar_listmonths.ViewIntersects(cal))
                        calendar_listmonths.Items.Add(cal);
                }

                calendar_listmonths.Invalidate();
            }
        }
        
        /// <summary>
        /// Set components status
        /// </summary>
        /// <param name="isEditing"></param>
        /// 
        private void SetCustomEditingMode(bool isEditing)
        {
            if (isEditing)
            {
                Program.uighfApplication.IsEditing = true;

                button_tabAppointments_edit.Enabled = false;
                button_tabAppointments_delete.Enabled = false;

                button_tabAppointments_save.Enabled = true;
                button_tabAppointments_cancel.Enabled = true;

                patients_idComboBox.Enabled = true;
                doctors_idComboBox.Enabled = true;
                rooms_idComboBox.Enabled = true;
                appointments_titleTextBox.ReadOnly = false;
                appointments_dateDateTimePicker.Enabled = true;
                appointments_fromDateTimePicker.Enabled = true;
                appointments_dateDateTimePicker.Enabled = true;
                appointments_toDateTimePicker.Enabled = true;
                appointments_notesTextBox.ReadOnly = false;

                tabControl_list.Enabled = false;

                panel_filters.Enabled = false;
            }
            else
            {
                Program.uighfApplication.IsEditing = false;

                button_tabAppointments_edit.Enabled = true;
                button_tabAppointments_delete.Enabled = true;

                button_tabAppointments_save.Enabled = false;
                button_tabAppointments_cancel.Enabled = false;

                patients_idComboBox.Enabled = false;
                doctors_idComboBox.Enabled = false;
                rooms_idComboBox.Enabled = false;
                appointments_titleTextBox.ReadOnly = true;
                appointments_dateDateTimePicker.Enabled = false;
                appointments_fromDateTimePicker.Enabled = false;
                appointments_dateDateTimePicker.Enabled = false;
                appointments_toDateTimePicker.Enabled = false;
                appointments_notesTextBox.ReadOnly = true;

                tabControl_list.Enabled = true;

                panel_filters.Enabled = true;
            }
        }

        /// <summary>
        /// Main tabcontrol
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_list_SelectedIndexChanged(object sender, EventArgs e)
        {
            IsBindingSourceLoading = true;
            _selectedAppointmentId = -1;
            appointmentsBindingSource.DataSource = new appointments();
            IsBindingSourceLoading = false;

            LoadAppointments();
        }

        /// <summary>
        /// Main BindingSource changed handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void appointmentsBindingSource_CurrentChanged(object sender, EventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            if (appointmentsBindingSource.Current != null)
            {
                _selectedAppointmentId = ((appointments)appointmentsBindingSource.Current).appointments_id;
                if (_selectedAppointmentId == 0)
                    _selectedAppointmentId = -1;
            }
        }
        
        #endregion

        
        #region tabAppointments

        /// <summary>
        /// Edit button click handlers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_tabAppointments_edit_Click(object sender, EventArgs e)
        {
            if (_selectedAppointmentId != -1)
            {
                _currentEditingMode = EditingMode.U;
                SetCustomEditingMode(true);

                doctors_idComboBox.Enabled = false;
            }
        }

        /// <summary>
        /// Delete button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_tabAppointments_delete_Click(object sender, EventArgs e)
        {
            if (_selectedAppointmentId != -1)
            {
                _currentEditingMode = EditingMode.D;
                button_tabAppointments_save_Click(sender, e);
            }
        }

        /// <summary>
        /// Save button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_tabAppointments_save_Click(object sender, EventArgs e)
        {
            if (appointments_fromDateTimePicker.Value.Minute <= 30)
                appointments_fromDateTimePicker.Value = new DateTime(appointments_fromDateTimePicker.Value.Year, appointments_fromDateTimePicker.Value.Month, appointments_fromDateTimePicker.Value.Day, appointments_fromDateTimePicker.Value.Hour, 0, 0);
            else
                appointments_fromDateTimePicker.Value = new DateTime(appointments_fromDateTimePicker.Value.Year, appointments_fromDateTimePicker.Value.Month, appointments_fromDateTimePicker.Value.Day, appointments_fromDateTimePicker.Value.Hour, 30, 0);

            if (appointments_toDateTimePicker.Value.Minute <= 30)
                appointments_toDateTimePicker.Value = new DateTime(appointments_toDateTimePicker.Value.Year, appointments_toDateTimePicker.Value.Month, appointments_toDateTimePicker.Value.Day, appointments_toDateTimePicker.Value.Hour, 0, 0);
            else
                appointments_toDateTimePicker.Value = new DateTime(appointments_toDateTimePicker.Value.Year, appointments_toDateTimePicker.Value.Month, appointments_toDateTimePicker.Value.Day, appointments_toDateTimePicker.Value.Hour, 30, 0);

            DateTime from = new DateTime(appointments_dateDateTimePicker.Value.Year, appointments_dateDateTimePicker.Value.Month, appointments_dateDateTimePicker.Value.Day, appointments_fromDateTimePicker.Value.Hour, appointments_fromDateTimePicker.Value.Minute, 00);
            DateTime to = new DateTime(appointments_dateDateTimePicker.Value.Year, appointments_dateDateTimePicker.Value.Month, appointments_dateDateTimePicker.Value.Day, appointments_toDateTimePicker.Value.Hour, appointments_toDateTimePicker.Value.Minute, 00);

            ((appointments)appointmentsBindingSource.Current).appointments_from = from;
            ((appointments)appointmentsBindingSource.Current).appointments_to = to;

            appointmentsBindingSource.EndEdit();

            try
            {
                if (_currentEditingMode == EditingMode.C)
                    DGUIGHFData.Add<appointments, DentneDModel>(_dentnedModel.Appointments, (appointments)appointmentsBindingSource.Current);
                else if (_currentEditingMode == EditingMode.U)
                    DGUIGHFData.Update<appointments, DentneDModel>(_dentnedModel.Appointments, (appointments)appointmentsBindingSource.Current);
                else if (_currentEditingMode == EditingMode.D)
                    DGUIGHFData.Remove<appointments, DentneDModel>(_dentnedModel.Appointments, (appointments)appointmentsBindingSource.Current);
            }
            catch (ArgumentException ex)
            {
                new DGUIGHFFormErrors(ex.Message, true).ShowDialog();
            }
            catch (DataException ex)
            {
                new DGUIGHFFormErrors(ex.Message, false).ShowDialog();
            }

            _currentEditingMode = EditingMode.R;
            SetCustomEditingMode(false);

            monthView_filterDay.SetDate(appointments_dateDateTimePicker.Value);

            LoadAppointments();
        }

        /// <summary>
        /// Cancel button click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_tabAppointments_cancel_Click(object sender, EventArgs e)
        {
            appointmentsBindingSource.CancelEdit();

            _currentEditingMode = EditingMode.R;
            SetCustomEditingMode(false);

            LoadAppointments();
        }

        /// <summary>
        /// Combobox autocomplete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void patients_idComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            DGUIGHFUtilsUI.DGComboBoxAutoComplete.OnKeyPress((ComboBox)sender, e);
        }

        /// <summary>
        /// Combobox autocomplete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void doctors_idComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            DGUIGHFUtilsUI.DGComboBoxAutoComplete.OnKeyPress((ComboBox)sender, e);
        }

        /// <summary>
        /// Combobox autocomplete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rooms_idComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            DGUIGHFUtilsUI.DGComboBoxAutoComplete.OnKeyPress((ComboBox)sender, e);
        }

        #endregion


        #region calendar handlers

        private int _verticalscrollY = 0;


        /// <summary>
        /// Day item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_ItemClick(object sender, CalendarItemEventArgs e)
        {
            if(_calendarDoubleClick)
            {
                _calendarDoubleClick = false;
            }
            else
            {
                if(tabControl_list.SelectedTab == tabPage_tabListDays)
                    tabPage_tabListDays.AutoScrollPosition = new Point(0, _verticalscrollY);
                else if (tabControl_list.SelectedTab == tabPage_tabListMonths)
                    tabPage_tabListMonths.AutoScrollPosition = new Point(0, _verticalscrollY);

                int appointments_id = ((CustomCalendarItem)e.Item).AppointmentId;
                if (appointments_id != -1)
                {
                    appointmentsBindingSource.DataSource = _dentnedModel.Appointments.Find(appointments_id);

                    SetCustomEditingMode(false);
                }
            }
        }

        /// <summary>
        /// Day item creating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_ItemCreating(object sender, CalendarItemCancelEventArgs e)
        {
            if (comboBox_filterDoctors.SelectedIndex != -1 && comboBox_filterDoctors.SelectedIndex != 0)
            {
                _currentEditingMode = EditingMode.C;
                SetCustomEditingMode(true);

                IsBindingSourceLoading = true;
                _selectedAppointmentId = -1;
                appointmentsBindingSource.DataSource = new appointments();
                appointmentsBindingSource.AddNew();
                IsBindingSourceLoading = false;

                appointments_dateDateTimePicker.Value = e.Item.StartDate;
                appointments_fromDateTimePicker.Value = e.Item.StartDate;
                appointments_toDateTimePicker.Value = e.Item.StartDate;
                if (e.Item.StartDate.Minute < 30)
                {
                    appointments_fromDateTimePicker.Value = new DateTime(e.Item.StartDate.Year, e.Item.StartDate.Month, e.Item.StartDate.Day, e.Item.StartDate.Hour, 0, 0);
                    appointments_toDateTimePicker.Value = new DateTime(e.Item.StartDate.Year, e.Item.StartDate.Month, e.Item.StartDate.Day, e.Item.StartDate.Hour, 0, 0).AddMinutes(30);
                }
                else
                {
                    appointments_fromDateTimePicker.Value = new DateTime(e.Item.StartDate.Year, e.Item.StartDate.Month, e.Item.StartDate.Day, e.Item.StartDate.Hour, 30, 0);
                    appointments_toDateTimePicker.Value = new DateTime(e.Item.StartDate.Year, e.Item.StartDate.Month, e.Item.StartDate.Day, e.Item.StartDate.Hour, 30, 0).AddMinutes(30);
                }

                ((appointments)appointmentsBindingSource.Current).doctors_id = Convert.ToInt32(((DGUIGHFUtilsUI.DGComboBoxItem)comboBox_filterDoctors.SelectedItem).Id);
                doctors_idComboBox.Enabled = false;
                               
                if (comboBox_filterRooms.SelectedIndex != -1 && comboBox_filterRooms.SelectedIndex != 0)
                    ((appointments)appointmentsBindingSource.Current).rooms_id = Convert.ToInt32(((DGUIGHFUtilsUI.DGComboBoxItem)comboBox_filterRooms.SelectedItem).Id);

                appointmentsBindingSource.ResetBindings(true);
            }

            e.Cancel = true;
        }

        /// <summary>
        /// Day double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_ItemDoubleClick(object sender, CalendarItemEventArgs e)
        {
            _calendarDoubleClick = true;
            calendar_listdays_ItemCreating(sender, new CalendarItemCancelEventArgs(e.Item));
        }

        /// <summary>
        /// Day item deleting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_ItemDeleting(object sender, CalendarItemCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Day item selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_ItemSelected(object sender, CalendarItemEventArgs e)
        { }
        
        /// <summary>
        /// Day painting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listdays_Paint(object sender, PaintEventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            if (calendar_listdays.GetSelectedItems().Count() > 1 || calendar_listdays.GetSelectedItems().Count() == 0)
            {
                IsBindingSourceLoading = true;
                _selectedAppointmentId = -1;
                appointmentsBindingSource.DataSource = new appointments();
                IsBindingSourceLoading = false;
            }
        }
        
        /// <summary>
        /// Weeks item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listweeks_ItemClick(object sender, CalendarItemEventArgs e)
        {
            calendar_listdays_ItemClick(sender, e);
        }

        /// <summary>
        /// Weeks item creating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listweeks_ItemCreating(object sender, CalendarItemCancelEventArgs e)
        {
            calendar_listdays_ItemCreating(sender, new CalendarItemCancelEventArgs(e.Item));
        }

        /// <summary>
        /// Weeks double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listweeks_ItemDoubleClick(object sender, CalendarItemEventArgs e)
        {
            _calendarDoubleClick = true;
            calendar_listdays_ItemCreating(sender, new CalendarItemCancelEventArgs(e.Item));
        }

        /// <summary>
        /// Week day header click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listweeks_DayHeaderClick(object sender, CalendarDayEventArgs e)
        {
            IsBindingSourceLoading = true;
            monthView_filterDay.SetDate(e.CalendarDay.Date);
            IsBindingSourceLoading = false;
            tabControl_list.SelectedTab = tabPage_tabListDays;
        }

        /// <summary>
        /// Weeks paiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listweeks_Paint(object sender, PaintEventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            if (calendar_listweeks.GetSelectedItems().Count() > 1 || calendar_listweeks.GetSelectedItems().Count() == 0)
            {
                IsBindingSourceLoading = true;
                _selectedAppointmentId = -1;
                appointmentsBindingSource.DataSource = new appointments();
                IsBindingSourceLoading = false;
            }
        }

        /// <summary>
        /// Month items Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listmonths_ItemClick(object sender, CalendarItemEventArgs e)
        {
            calendar_listdays_ItemClick(sender, e);
        }
        
        /// <summary>
        /// Month item creating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listmonths_ItemCreating(object sender, CalendarItemCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Month day header click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listmonths_DayHeaderClick(object sender, CalendarDayEventArgs e)
        {
            IsBindingSourceLoading = true;
            monthView_filterDay.SetDate(e.CalendarDay.Date);
            IsBindingSourceLoading = false;
            tabControl_list.SelectedTab = tabPage_tabListDays;
        }

        /// <summary>
        /// Month paiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void calendar_listmonths_Paint(object sender, PaintEventArgs e)
        {
            if (IsBindingSourceLoading)
                return;

            if (calendar_listmonths.GetSelectedItems().Count() > 1 || calendar_listmonths.GetSelectedItems().Count() == 0)
            {
                IsBindingSourceLoading = true;
                _selectedAppointmentId = -1;
                appointmentsBindingSource.DataSource = new appointments();
                IsBindingSourceLoading = false;
            }
        }

        /// <summary>
        /// Get a random color from number
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private Color getCalendarItemColor(int number)
        {
            Random randomGen = new Random(number);
            return Color.FromArgb(randomGen.Next(0, 255), randomGen.Next(0, 255), randomGen.Next(0, 255));
        }

        #endregion
                                                                               
    }

    #region Calendar custom objects

    /// <summary>
    /// Custom appointment item
    /// </summary>
    public class CustomAppointmentItem
    {
        /// <summary>
        /// Item date from
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Item date to
        /// </summary>
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Item title for days calendar
        /// </summary>
        public String TitleDay { get; set; }

        /// <summary>
        /// Item title for weeks calendar
        /// </summary>
        public String TitleWeek { get; set; }

        /// <summary>
        /// Item title for moths calendar
        /// </summary>
        public String TitleMonth { get; set; }

        /// <summary>
        /// Item color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Item appointment id
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// Item patient treatment id
        /// </summary>
        public int PatientTreatmentId { get; set; }
    }

    /// <summary>
    /// Custom calendar item
    /// </summary>
    public class CustomCalendarItem : CalendarItem
    {
        /// <summary>
        /// Custom costructor
        /// </summary>
        /// <param name="calendar"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="text"></param>
        public CustomCalendarItem(Calendar calendar, DateTime startDate, DateTime endDate, string text)
            : base(calendar, startDate, endDate, text)
        { }

        /// <summary>
        /// Item appointment id
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// Item patient treatment id
        /// </summary>
        public int PatientTreatmentId { get; set; }
    }

    #endregion

}