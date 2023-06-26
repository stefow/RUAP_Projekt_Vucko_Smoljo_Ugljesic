using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Fish_recognition
{

    public partial class Form1 : Form
    {
        private const string predictionEndpoint = "https://westeurope.api.cognitive.microsoft.com/customvision/v3.0/Prediction/55b1aa62-4294-4ffe-8998-7133f82f8dd4/classify/iterations/Iteration3/image"; // Replace with your Custom Vision prediction endpoint URL
        private const string predictionKey = "fa854d00151a4ad2b8328a59e11fe0de"; // Replace with your Custom Vision prediction key
        string imagePath = "";
        public Form1()
        {
            InitializeComponent();
            button1.Enabled = false;
        }
        public class Prediction
        {
            public double Probability { get; set; }
            public string TagName { get; set; }
        }
        public class PredictionResult
        {
            public List<Prediction> Predictions { get; set; }
        }
        public void PrintData(string data)
        {
            PredictionResult predictionResult = JsonConvert.DeserializeObject<PredictionResult>(data);
            label1.Text = "";
            int labelY = 20;
            groupBox1.Controls.Clear();
            foreach (Prediction prediction in predictionResult.Predictions)
            {
                string tagName = prediction.TagName;
                double probability = prediction.Probability;
                if (probability > 0.1)
                {
                    Label label = new Label();

                    label.Text = tagName + " - " + (probability * 100).ToString("0.00") + "%";
                    label.AutoSize = true;
                    label.Location = new Point(5, labelY);
                    if (probability > 0.5) label.ForeColor = Color.Green;
                    else label.ForeColor = Color.Red;
                    label.Font = new Font(label.Font.FontFamily, 14, label.Font.Style);
                    groupBox1.Controls.Add(label);
                    labelY += label.Height + 4;
                }

            }
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Visible = false;
                    imagePath = openFileDialog.FileName;
                    pictureBox1.ImageLocation = imagePath;
                    button1.Enabled = true;
                }
            }
        }
        private async Task PredictImage(string imagePath)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);

                    byte[] imageData = File.ReadAllBytes(imagePath);
                    using (ByteArrayContent content = new ByteArrayContent(imageData))
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                        HttpResponseMessage response = await client.PostAsync(predictionEndpoint, content);
                        response.EnsureSuccessStatusCode();

                        string result = await response.Content.ReadAsStringAsync();
                        PrintData(result);
                    }
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private async void Provjeri(object sender, EventArgs e)
        {
            await PredictImage(imagePath);
        }
        
    }
}