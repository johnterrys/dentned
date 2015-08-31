﻿#region License
// Copyright (c) 2015 Davide Gironi
//
// Please refer to LICENSE file for licensing information.
#endregion

using System.Linq;
using DG.Data.Model;
using DG.DentneD.Model.Entity;
using System;

namespace DG.DentneD.Model.Repositories
{
    public class ContactsTypesRepository : GenericDataRepository<contactstypes, DentneDModel>
    {
        public ContactsTypesRepository() : base() { }

        /// <summary>
        /// Repository language dictionary
        /// </summary>
        public class RepositoryLanguage : IGenericDataRepositoryLanguage
        {
            public string text001 = "Name can not be empty.";
            public string text002 = "Contact type already inserted.";
            public string text003 = "This item can not be removed. A contact depends it.";
        }

        /// <summary>
        /// Repository language
        /// </summary>
        public RepositoryLanguage language = new RepositoryLanguage();

        /// <summary>
        /// Check if an item can be added
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public override bool CanAdd(ref string[] errors, params contactstypes[] items)
        {
            errors = new string[] { };

            bool ret = Validate(false, ref errors, items);
            if (!ret)
                return ret;

            ret = base.CanAdd(ref errors, items);

            return ret;
        }

        /// <summary>
        /// Check if an item can be updated
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public override bool CanUpdate(ref string[] errors, params contactstypes[] items)
        {
            errors = new string[] { };

            bool ret = Validate(true, ref errors, items);
            if (!ret)
                return ret;

            ret = base.CanUpdate(ref errors, items);

            return ret;
        }

        /// <summary>
        /// Validate an item
        /// </summary>
        /// <param name="isUpdate"></param>
        /// <param name="errors"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        private bool Validate(bool isUpdate, ref string[] errors, params contactstypes[] items)
        {
            bool ret = true;

            foreach (contactstypes item in items)
            {
                if (String.IsNullOrEmpty(item.contactstypes_name))
                {
                    ret = false;
                    errors = errors.Concat(new string[] { language.text001 }).ToArray();
                }

                if (!ret)
                    break;

                if (!isUpdate)
                {
                    if (List(r => r.contactstypes_name == item.contactstypes_name).Count() > 0)
                    {
                        ret = false;
                        errors = errors.Concat(new string[] { language.text002 }).ToArray();
                    }
                }
                else
                {
                    if (List(r => r.contactstypes_id != item.contactstypes_id && r.contactstypes_name == item.contactstypes_name).Count() > 0)
                    {
                        ret = false;
                        errors = errors.Concat(new string[] { language.text002 }).ToArray();
                    }
                }

                if (!ret)
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Check if an item can be removed
        /// </summary>
        /// <param name="checkForeingKeys"></param>
        /// <param name="excludedForeingKeys"></param>
        /// <param name="errors"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public override bool CanRemove(bool checkForeingKeys, string[] excludedForeingKeys, ref string[] errors, params contactstypes[] items)
        {
            bool ret = true;

            errors = new string[] { };

            foreach (contactstypes item in items)
            {
                if (BaseModel.PatientsContacts.List(r => r.contactstypes_id == item.contactstypes_id).Count > 0)
                {
                    ret = false;
                    errors = errors.Concat(new string[] { language.text003 }).ToArray();
                }

                if (!ret)
                    break;
            }

            if (!ret)
                return ret;

            ret = base.CanRemove(checkForeingKeys, excludedForeingKeys, ref errors, items);

            return ret;
        }

    }

}

