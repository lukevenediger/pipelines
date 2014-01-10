using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Pipelines.Service.Core;
using Pipelines.Service.Core.Schedulers;

namespace Pipelines.Service
{
    public partial class Mailer : ServiceBase
    {

        public Mailer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var scheduler = new MailScheduler().GetScheduler();
            scheduler.Start();
        }

        protected override void OnStop()
        {

        }
    }
}
