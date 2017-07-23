using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using Plugin.Media.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ImageSense
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ComputerVision : ContentPage
    {
        public ComputerVision()
        {
            InitializeComponent();
        }

        async void LoadCamera(object sender, EventArgs e)
        {
            

            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }


            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions            //crashes/exception here
            {

                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"

            });


            if (file == null)
                return;


            image.Source = ImageSource.FromStream(() =>
            {

                return file.GetStream();
            });


            await MakePredictionRequest(file);
        }


        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file) //this function calls api
        {
            var client = new HttpClient();

            //client.DefaultRequestHeaders.Add("Prediction-Key", "a51ac8a57d4e4345ab0a48947a4a90ac");

            //string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/4da1555c-14ca-4aaf-af01-d6e1e97e5fa6/image?iterationId=7bc76035-3825-4643-917e-98f9d9f79b71";

            client.DefaultRequestHeaders.Add("Prediction-Key", "430e3360ff90462a8cf63000ec2d2b54");
            string url = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Description&subscription-key=430e3360ff90462a8cf63000ec2d2b54";


            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);


                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject rss = JObject.Parse(responseString);

                    var caption = from p in rss["description"]["captions"] select (string)p["text"];
                    var captionToDB = "test";

                    foreach (var item in caption)
                    {
                        captionLabel.Text = item;
                        captionToDB = item;

                    }

                    ImageSenseModel model = new ImageSenseModel()
                    {
                        Caption = captionToDB                     

                    };

                    await AzureManager.AzureManagerInstance.PostImageSenseInformation(model);


                }
            }


            file.Dispose();


        }

    }
}
