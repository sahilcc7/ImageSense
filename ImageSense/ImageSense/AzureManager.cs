using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSense
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<ImageSenseModel> ImageSenseTable; //check this



        private AzureManager()
        {
            this.client = new MobileServiceClient("http://imagesense.azurewebsites.net"); 
            this.ImageSenseTable = this.client.GetTable<ImageSenseModel>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<ImageSenseModel>> GetImageSenseInformation()
        {
            return await this.ImageSenseTable.ToListAsync();
        }

        public async Task PostImageSenseInformation(ImageSenseModel imageSenseModel)
        {
            await this.ImageSenseTable.InsertAsync(imageSenseModel);
        }
        

    }
}
