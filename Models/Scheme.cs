﻿namespace GovConnect.Models
{
    public class Scheme
    {
        public int SchemeID { get; set; }            // Auto-incrementing primary key
        public string SchemeName { get; set; }
        public string Details { get; set; }          // Column to store scheme details
        public String Attributes { get; set; }
        public string Eligibility { get; set; }      // Column to store eligibility criteria
        public string ApplicationProcess { get; set; } // Column to store the application process steps
        public string DocsRequired { get; set; }     // Column to store required documents
        
    }

}
