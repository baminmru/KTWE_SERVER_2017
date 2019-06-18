using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TestDyn
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void CmdTest_Click(object sender, EventArgs e)
        {

            var dyn = JsonConvert.DeserializeObject<dynamic>(txtJSON.Text);

            System.Diagnostics.Debug.Print(dyn.data[0].ToString());
        }
    }
}
