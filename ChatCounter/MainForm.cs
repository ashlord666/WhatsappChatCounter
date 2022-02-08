using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChatCounter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        MainForm(object sender, EventArgs e)
        {

        }

        private void ChooseFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog chooseWhatsAppChatDialog = new OpenFileDialog();
            chooseWhatsAppChatDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            chooseWhatsAppChatDialog.Multiselect = false;
            chooseWhatsAppChatDialog.ShowDialog();
            whatsAppChatFilename.Text = chooseWhatsAppChatDialog.FileName;
            statusMsg.Text = "";
        }

        private void GenerateCSVButton_Click(object sender, EventArgs e)
        {
            ChooseFileButton.Enabled = false;
            //statusMsg.Text = $"startDate: {startDateTimePicker.Value.ToShortDateString()}, endDate: {endDateTimePicker.Value.Date.ToShortDateString()}";

            Regex rgx = new Regex(@"^(\d+/\d+/\d+),\s+\d{2}:\d{2}\s+[^-]+\s+-\s+([^:]+):.*$");
            int totalcount = 0;
            int matchedcount = 0;
            int datematchedcount = 0;
            var cultureInfo = new CultureInfo("en-US");
            IDictionary<string, int> talkative = new Dictionary<string, int>();

            try
            {
                if (endDateTimePicker.Value.Date < startDateTimePicker.Value.Date)
                {
                    MessageBox.Show("Don't make me insult your intelligence but your start and end dates don't make sense", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusMsg.Text = "";
                }
                else
                {
                    if (whatsAppChatFilename.Text.Trim().Length > 0)
                    {
                        if (File.Exists(whatsAppChatFilename.Text))
                        {
                            foreach (string line in File.ReadLines(whatsAppChatFilename.Text))
                            {
                                totalcount++;

                                if (rgx.IsMatch(line))
                                {
                                    matchedcount++;
                                    Match match = rgx.Match(line);

                                    DateTime dateValue;

                                    // Parse date
                                    if (DateTime.TryParseExact(match.Groups[1].Value, "dd/MM/yy", cultureInfo, DateTimeStyles.None, out dateValue))
                                    {
                                        // Check date
                                        if ((dateValue.Date > endDateTimePicker.Value.AddDays(1).Date) || (dateValue.Date < startDateTimePicker.Value.Date))
                                        {
                                            continue;
                                        }

                                        // Increment count
                                        talkative.TryGetValue(match.Groups[2].Value, out var currentCount);
                                        talkative[match.Groups[2].Value] = currentCount + 1;
                                        datematchedcount++;
                                    }

                                    // Else, ignore the line since the date cannot be parsed. This should never happen though.
                                }

                            }

                            var ordered = talkative.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                            var outputfile = Path.Combine(Path.GetDirectoryName(whatsAppChatFilename.Text), $"{Path.GetFileNameWithoutExtension(whatsAppChatFilename.Text)}.csv");

                            using (TextWriter tw = new StreamWriter(outputfile, append: false))
                            {
                                ordered.ToList().ForEach(x => tw.WriteLine($"{x.Key},{x.Value}"));
                            }

                            statusMsg.Text = $"{datematchedcount} lines filtered from {matchedcount} messages. Processed {totalcount} lines. {ordered.ToList().Count} chatters.";

                        }
                        else
                        {
                            MessageBox.Show("File doesn't exist...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            statusMsg.Text = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ChooseFileButton.Enabled = true;
        }
    }
}
