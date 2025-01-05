using NAudio.Lame;
using NAudio.Wave;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader
{
    public partial class mainForm : Form
    {
        public mainForm()
        {
            InitializeComponent();
        }

        string folderPath = "";

        private void button1_Click(object sender, EventArgs e)
        {
            button1_ClickAsync(sender, e);
            // StatusLabel'a bilgi mesajı ekleyin
      


        }



        private async void button1_ClickAsync(object sender, EventArgs e)
        {

            // YouTube URL'sini al
            string videoUrl = textBox1.Text.Trim();

            // Eğer URL boşsa uyarı ver
            if (string.IsNullOrEmpty(videoUrl))
            {
                MessageBox.Show("Lütfen bir YouTube URL'si girin.");
                return;
            }


            // Klasör seçmek için FolderBrowserDialog kullanıyoruz
            if (string.IsNullOrEmpty(folderPath))
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Bir klasör seçin";
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        folderPath = folderDialog.SelectedPath;
                   
                    }
                }
            }
            await DownloadAndConvertToMp3(videoUrl, folderPath);
        }

        private async Task DownloadAndConvertToMp3(string videoUrl, string folderPath)
        {
            button1.Enabled = false;
            try
            {
                this.Text = "Video İndiriliyor..";
                // YouTubeExplode kütüphanesi ile video bilgilerini al
                var youtube = new YoutubeClient();
                Application.DoEvents();
                var video = await youtube.Videos.GetAsync(videoUrl);
                Application.DoEvents();
                // Video başlığını dosya adı olarak kullan
                string videoTitle = video.Title;
                string validFileName = GetValidFileName(videoTitle); // Geçerli bir dosya adı oluştur

                // Klasörde video başlığına göre dosya yolu
                string tempFilePath = Path.Combine(folderPath, validFileName + ".mp4");

                // Video akışlarını al
                var streamInfo = await youtube.Videos.Streams.GetManifestAsync(videoUrl);
                var audioStreamInfo = streamInfo.GetAudioStreams().GetWithHighestBitrate();
                Application.DoEvents();
                // Akışı indir
                var audioStream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);

                // Video dosyasını geçici olarak kaydet
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await audioStream.CopyToAsync(fileStream);
                }
                this.Text = "Video İndirildi.. Mp3 Dönüştürülüyor..";

                // MP3 dosyasına dönüştürme işlemi
                string mp3FilePath = Path.Combine(folderPath, validFileName + ".mp3");
                ConvertToMp3(tempFilePath, mp3FilePath);

                // Geçici MP4 dosyasını sil
                File.Delete(tempFilePath);
                this.Text = "Video başarıyla indirildi ve MP3 formatında kaydedildi.";
                //MessageBox.Show("Video başarıyla indirildi ve MP3 formatında kaydedildi.");
                textBox1.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private void ConvertToMp3(string filePath, string mp3FilePath)
        {
            try
            {

                // MP4 dosyasını MP3'e dönüştürme (NAudio ile)
                using (var reader = new MediaFoundationReader(filePath))
                {
                    Application.DoEvents();
                    using (var writer = new LameMP3FileWriter(mp3FilePath, reader.WaveFormat, 128))
                    {
                        reader.CopyTo(writer);
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dönüştürme hatası: {ex.Message}");
            }
        }

        // Dosya adında geçerli olmayan karakterleri kaldırmak için yardımcı fonksiyon
        private string GetValidFileName(string fileName)
        {
            // Geçerli dosya adı oluşturmak için geçersiz karakterleri temizleyelim
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Klasör seçmek için FolderBrowserDialog kullanıyoruz
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Bir klasör seçin";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPath = folderDialog.SelectedPath;
                }
            }
        }
    }
}
