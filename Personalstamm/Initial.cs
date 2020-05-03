using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Personalstamm
{
    public partial class Initial : Form
    {

        private string DataPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data.json");
        private ResourceManager ResManager = new ResourceManager("Personalstamm.Message", Assembly.GetExecutingAssembly());

        public Initial()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            this.Controls.Clear();
            this.InitializeComponent();

            LoadMainDataPanel();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");
            this.Controls.Clear();
            this.InitializeComponent();

            LoadMainDataPanel();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void mainPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //changed list
            var updatedResultList = ReadDataSource();

            //saved list
            var savedResultList = ReadSavedJson();

            //defining difference and updating date
            var changedRecords = savedResultList.Except(updatedResultList, new RecordComparer()).ToList();

            foreach (var item in updatedResultList.Where(n => changedRecords.Any(l => l.Personalnummer == n.Personalnummer)))
            {
                item.Aenderungsdatum = DateTime.Now;
            }

            SaveJson(updatedResultList);
            //refresh the grid
            loadData();

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            UpdateDataGridCOntrolsVisibility();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(textBox2.Text) && !string.IsNullOrEmpty(textBox3.Text))
            {
                var personalNumber = textBox1.Text;
                var name = textBox2.Text;
                float salary;
                float.TryParse(textBox3.Text, out salary);
                byte[] imageByteArray = null;
                if (File.Exists(openFileDialog1.FileName) && CheckIfFileIsImage(openFileDialog1.FileName))
                {
                    Image image = Image.FromFile(openFileDialog1.FileName);
                    var resizedImage = Imager.Resize(image, 200, 300, true);
                    imageByteArray = image != null ? imageToByteArray(resizedImage) : null;
                }

                //new JsonRecord object 
                var record = new JsonRecord()
                {
                    Personalnummer = personalNumber,
                    Name = name,
                    Gehalt = salary,
                    Aenderungsdatum = DateTime.Now,
                    Bild = imageByteArray
                };

                var input = File.ReadAllText(DataPath);
                var results = JsonConvert.DeserializeObject<List<JsonRecord>>(input);
                var duplicates = results.Where(x => x.Personalnummer == personalNumber);

                results.Add(record);

                SaveJson(results);

                loadData();

                HideAddNewRecordPanel();

                ClearRecordBoxesValues();
            }
            else
            {
                MessageBox.Show("Empty values are not allowed!");
                button5.Enabled = false;
            }
        }

        private void ClearRecordBoxesValues()
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            openFileDialog1.FileName = string.Empty;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            HideAddNewRecordPanel();
            ClearRecordBoxesValues();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var filePath = openFileDialog1.FileName;
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void textBox1_Validating(object sender, CancelEventArgs e)
        {
            if (!ValidateDuplicates() || !ValidateEmptyValues(textBox1))
            {
                button5.Enabled = false;
            }
            else
                button5.Enabled = true;

        }

        private void textBox2_Validating(object sender, CancelEventArgs e)
        {
            if (!ValidateDuplicates() || !ValidateEmptyValues(textBox2))
            {
                button5.Enabled = false;
            }
            else
                button5.Enabled = true;
        }

        private void textBox3_Validating(object sender, CancelEventArgs e)
        {
            if (!ValidateDuplicates() || !ValidateEmptyValues(textBox3))
            {
                button5.Enabled = false;
            }
            else
                button5.Enabled = true;
        }

        /// <summary>
        /// Loads DataGridView
        /// </summary>
        private void loadData()
        {
            List<JsonRecord> result = ReadSavedJson();
            this.dataGridView1.DataSource = result;
            dataGridView1.Columns[4].ToolTipText = ResManager.GetString("browseImageNote");
            dataGridView1.Columns["Personalnummer"].ReadOnly = true;
            dataGridView1.Columns["Aenderungsdatum"].ReadOnly = true;
            dataGridView1.Width = panel1.Width;
            dataGridView1.Refresh();


            ContextMenuStrip m = new ContextMenuStrip();
            ToolStripMenuItem mnuDelete = new ToolStripMenuItem(ResManager.GetString("deleteContextMenu"));
            m.Items.Add(mnuDelete);
            dataGridView1.Columns[4].ContextMenuStrip = m;
            mnuDelete.Click += new EventHandler(mnuDelete_Click);
        }

        private List<JsonRecord> ReadSavedJson()
        {
            var input = File.ReadAllText(DataPath, Encoding.UTF8);
            var result = JsonConvert.DeserializeObject<List<JsonRecord>>(input);
            return result;
        }

        /// <summary>
        /// Converts image object to byte array
        /// </summary>
        /// <param name="imageIn"></param>
        /// <returns></returns>
        private byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }


        /// <summary>
        /// Validates personal number duplicates
        /// </summary>
        /// <returns></returns>
        private bool ValidateDuplicates()
        {
            bool status = true;
            var results = ReadSavedJson();
            var duplicates = results.Where(x => x.Personalnummer == textBox1.Text);

            // Retrieve the value of the string resource named "errorText".
            // The resource manager will retrieve the value of the  
            // localized resource using the caller's current culture setting.

            if (duplicates.Any())
            {
                errorProvider1.SetError(textBox1, ResManager.GetString("duplicateErrorText"));
                status = false;
            }
            else
            {
                errorProvider1.Clear();
            }
            return status;
        }

        /// <summary>
        /// Verifies if file is an image
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool CheckIfFileIsImage(string path)
        {
            List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

            if (ImageExtensions.Contains(Path.GetExtension(path).ToUpperInvariant()))
            {
                return true;
            }
            return false;
        }

        private bool ValidateEmptyValues(TextBox textBox)
        {
            bool status = true;

            if (string.IsNullOrEmpty(textBox.Text))
            {
                errorProvider1.SetError(textBox, ResManager.GetString("emptyErrorText"));
                status = false;
            }
            else
            {
                errorProvider1.Clear();
            }
            return status;
        }

        private void button5_CausesValidationChanged(object sender, EventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var result = ReadSavedJson();

            foreach (DataGridViewRow item in this.dataGridView1.SelectedRows)
            {
                var confirmResult = MessageBox.Show(ResManager.GetString("confirmationDeleteMessage"),
                                     ResManager.GetString("confirmDelete"),
                                     MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    result.RemoveAt(item.Index);
                }
                else
                {
                }

            }
            SaveJson(result);
            this.dataGridView1.DataSource = result;


            dataGridView1.Refresh();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentCell.ColumnIndex == 4)
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var filePath = openFileDialog1.FileName;
                        byte[] imageByteArray = null;


                        if (File.Exists(openFileDialog1.FileName) && CheckIfFileIsImage(openFileDialog1.FileName))
                        {
                            Image image = Image.FromFile(openFileDialog1.FileName);
                            var resizedImage = Imager.Resize(image, 200, 300, true);
                            imageByteArray = image != null ? imageToByteArray(resizedImage) : null;
                        }
                        openFileDialog1.FileName = string.Empty;
                        dataGridView1.CurrentCell.Value = imageByteArray;
                        var resultList = ReadDataSource();
                        SaveJson(resultList);
                        //refresh the grid
                        loadData();
                    }
                    catch (SecurityException ex)
                    {
                        MessageBox.Show($"Error.\n\nError message: {ex.Message}\n\n" +
                        $"Details:\n\n{ex.StackTrace}");
                    }
                }
            }
        }

        private void SaveJson(List<JsonRecord> resultList)
        {
            string outputToSave = JsonConvert.SerializeObject(resultList);
            System.IO.File.WriteAllText(DataPath, outputToSave);
        }

        private List<JsonRecord> ReadDataSource()
        {
            string output = JsonConvert.SerializeObject(this.dataGridView1.DataSource);
            var resultList = JsonConvert.DeserializeObject<List<JsonRecord>>(output);
            return resultList;
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

        }

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentRow.Cells[4].Value = null;
        }

        /// <summary>
        /// Checks changes in the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Initial_FormClosing(object sender, FormClosingEventArgs e)
        {
            var savedValues = ReadSavedJson();
            var currentValues = ReadDataSource();

            var changedRecords = currentValues == null ? new List<JsonRecord>() : savedValues.Except(currentValues, new RecordComparer()).ToList();

            if (changedRecords.Any())
            {
                DialogResult dlg = MessageBox.Show(ResManager.GetString("saveChangesMessage"),
                                    ResManager.GetString("saveChanges"), MessageBoxButtons.YesNoCancel);

                if (dlg == DialogResult.Yes)
                {
                    SaveJson(currentValues);

                    e.Cancel = false;

                }
                if (dlg == DialogResult.No)
                {
                    e.Cancel = false;
                }
                if (dlg == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Updates grid visibility
        /// </summary>
        private void UpdateDataGridCOntrolsVisibility()
        {
            panel2.Visible = true;
            button7.Visible = false;
            button8.Visible = false;
            button3.Visible = false;
        }

        /// <summary>
        /// Loads gridview
        /// </summary>
        private void LoadMainDataPanel()
        {
            mainPanel.Visible = true;
            groupBox2.Visible = false;
            loadData();
        }

        /// <summary>
        /// Hides add new record panel
        /// </summary>
        private void HideAddNewRecordPanel()
        {
            panel2.Visible = false;
            button7.Visible = true;
            button8.Visible = true;
            button3.Visible = true;
        }
    }
}
