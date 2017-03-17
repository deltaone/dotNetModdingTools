using System;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace dotNetMT
{
    public partial class FrameSetup : Form
    {
        public FrameSetup()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            
            var items = clbPlugins.Items;

            foreach (var item in Core.modPluginList.OrderBy(key => key.Value.ToString())) items.Add(item.Value, item.Value.active);            
            //foreach (var k in Core.modPluginList.Keys) items.Add(Core.modPluginList[k], Core.modPluginList[k].active);

            if (items.Count > 0)
            {
                tbCreatedBy.Text = ((PluginEntry)items[0]).pluginAuthor;
                tbSummary.Text = ((PluginEntry)items[0]).pluginNote;
                clbPlugins.SetSelected(0, true);
            }

            btnUninstall.Enabled = Core.modInstalled;
            btnInstall.Enabled = !Core.modInstalled;
            clbPlugins.Enabled = !Core.modInstalled;
            btnSelectAll.Enabled = !Core.modInstalled;
            btnSelectNone.Enabled = !Core.modInstalled;
            tbSummary.Enabled = !Core.modInstalled;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbPlugins.Items.Count; i++)
            {
                var entry = clbPlugins.Items[i] as PluginEntry;
                entry.active = clbPlugins.GetItemChecked(i);
            }            
            Core.modSetupAction = 1;            
            Close();            
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            Core.modSetupAction = 2;
            Close();
        }

        private void clbPlugins_SelectedIndexChanged(object sender, EventArgs e)
        {
            var items = clbPlugins.Items;
            tbCreatedBy.Text = ((PluginEntry)items[clbPlugins.SelectedIndex]).pluginAuthor;
            tbSummary.Text = ((PluginEntry)items[clbPlugins.SelectedIndex]).pluginNote;
        }

        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbPlugins.Items.Count; i++) clbPlugins.SetItemChecked(i, false);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < clbPlugins.Items.Count; i++) clbPlugins.SetItemChecked(i, true);
        }

        private void clbPlugins_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!this.IsHandleCreated) return;

            this.BeginInvoke((MethodInvoker)delegate
            {
                CheckedListBox clb = (CheckedListBox)sender;
                if (e.NewValue == CheckState.Checked)
                {
                    string requiredPludin = ((PluginEntry)clb.Items[e.Index]).pluginRequired;
                    if (requiredPludin != "")
                        for (int i = 0; i < clb.Items.Count; i++)
                            if (((PluginEntry)clb.Items[i]).pluginTag == requiredPludin && clb.GetItemChecked(i) == false)
                            {
                                clb.SetItemCheckState(e.Index, CheckState.Unchecked);
                                MessageBox.Show("Required plugin '" + clb.Items[e.Index].ToString() + "' not enabled!", "Warning!");
                                break;
                            }
                }
                else if (e.NewValue == CheckState.Unchecked)
                {
                    string requiredPludin = ((PluginEntry)clb.Items[e.Index]).pluginTag;
                    for (int i = 0; i < clb.Items.Count; i++)
                        if (((PluginEntry)clb.Items[i]).pluginRequired == requiredPludin)
                            clb.SetItemCheckState(i, CheckState.Unchecked);
                }                
            });
            
            //clb.ItemCheck -= clbPlugins_ItemCheck;
            //clb.SetItemCheckState(e.Index, e.NewValue);
            //clb.ItemCheck += clbPlugins_ItemCheck;           
        }
    }
}
