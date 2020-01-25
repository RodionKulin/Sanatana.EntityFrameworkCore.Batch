using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Sanatana.EntityFrameworkCore.Batch.Scripts
{
    /// <summary>
    /// Database initializer that ensures database is created and executes sql scripts on database creation.
    /// </summary>
    public class ScriptInitializer
    {
        //properties
        public Dictionary<string, string> Replacements { get; set; }
        public ScriptManager ScriptManager { get; private set; }


        //init
        public ScriptInitializer(DirectoryInfo scriptDirectory, Dictionary<string, string> replacements = null)
        {
            ScriptManager = new ScriptManager(scriptDirectory);
            Replacements = replacements;
        }
       
        public ScriptInitializer(Type scriptResourceType, Dictionary<string, string> replacements = null)
        {
            ScriptManager = new ScriptManager(scriptResourceType);
            Replacements = replacements;
        }




        //methods
        public void InitializeDatabase(DbContext context)
        {
            bool created = context.Database.EnsureCreated();

            if (created)
            {
                ExecuteScripts(context);
            }
        }
        
        public virtual void ExecuteScripts(DbContext context)
        {
            var replacements = Replacements ?? new Dictionary<string, string>();

            foreach (SqlScript item in ScriptManager.Scripts)
            {
                item.Replacement = replacements;
            }

            ScriptManager.ExecuteScripts(context);
        }
    }
}