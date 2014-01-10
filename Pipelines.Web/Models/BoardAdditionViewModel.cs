using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pipelines.Web.Models
{
    public class BoardAdditionViewModel
    {
        public string Name { get; set; }
        public List<string> Stages { get; set; }
        public List<string> Pipelines { get; set; }
    }
}