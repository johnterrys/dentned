//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DG.DentneD.Model.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class patientsattributes
    {
        public int patientsattributes_id { get; set; }
        public int patients_id { get; set; }
        public int patientsattributestypes_id { get; set; }
        public string patientsattributes_value { get; set; }
        public string patientsattributes_note { get; set; }
    
        public virtual patients patients { get; set; }
        public virtual patientsattributestypes patientsattributestypes { get; set; }
    }
}
