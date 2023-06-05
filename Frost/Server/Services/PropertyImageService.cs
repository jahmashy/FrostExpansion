using Frost.Server.Services.Interfaces;

namespace Frost.Server.Services
{
    public class PropertyImageService : IPropertyImageService
    {
        public void GetPropertyImages(int property_id)
        {
            string dirPath = $"Resources/PropertyImages/{property_id}";

            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists ) {
                Console.WriteLine("ni ma");
            }
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo file in files)
            {
                Console.WriteLine(file.Name);
            }
            
            
        }
    }
}
