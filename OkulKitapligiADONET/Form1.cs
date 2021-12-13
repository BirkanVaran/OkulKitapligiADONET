using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OkulKitapligiADONET
{
    public partial class FormYazarlar : Form
    {
        public FormYazarlar()
        {
            InitializeComponent();
        }

        // GLOBAL ALAN

        // SQL CONNECTION Nesnesi: SQL Veritabanıyla bağlantı kurmak için kullanacağımız Class'tır.
        // System.Data.Client namespace'i içinde yer alır.

        SqlConnection baglanti = new SqlConnection();
        string SQLBaglantiCumlesi = @"Server=DESKTOP-HNE43R2;Database=OKULKITAPLIGI;Trusted_Connection=True;";


        private void FormYazarlar_Load(object sender, EventArgs e)
        {
            dataGridViewYazarlar.MultiSelect = false; // Çoklu satır seçimini engelledik.
            dataGridViewYazarlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // DataGrid üzerinde bir hücreye tıklandığında tüm satırı seçecek.

            dataGridViewYazarlar.ContextMenuStrip = contextMenuStrip1;


            // Grid'in içine bilgileri getirelim:
            baglanti.ConnectionString = SQLBaglantiCumlesi;


            //Grid'in içine bilgileri getirelim:
            TumYazarlariGetir();


        }

        private void TumYazarlariGetir()
        {
            try
            {
                // SQLConnection'a bağlanacağı adresi verdik.

                // SQLCOMMAND Nesnesi: Sorgularımıza ve porsedürlerimize ait komutları ala nesnedir.
                SqlCommand komut = new SqlCommand();
                komut.Connection = baglanti;
                komut.CommandType = CommandType.Text;
                string sorgu = "SELECT * FROM Yazarlar WHERE SilindiMi=0 ORDER BY YazarID DESC";
                komut.CommandText = sorgu;


                BaglantiyiAc();

                SqlDataAdapter adaptor = new SqlDataAdapter(komut); /* SQLDataAdapter: sorgu çalışınca oluşan dataların aktarılması işlemini yapar.
                                                                     * Adapter'a hangi komunutun işleneceğini şu an olduğu gibi CTOR'da ya da*  sonradan verebiliriz. */

                // Ya da* sonradan verirsek:
                //SqlDataAdapter adaptor = new SqlDataAdapter();
                //adaptor.SelectCommand = komut;

                // Adaptor'un içindeki verileri sanalTablo'ya aktaralım:
                DataTable sanalTablo = new DataTable();
                adaptor.Fill(sanalTablo);
                dataGridViewYazarlar.DataSource = sanalTablo;
                dataGridViewYazarlar.Columns["SilindiMi"].Visible = false;
                dataGridViewYazarlar.Columns["YazarAdSoyad"].Width = 230;

                BaglantiyiKapat();
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Beklenmedik bir hata oluştu! HATA: {ex.Message}", "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            switch (btnEkle.Text)
            {
                case "EKLE":
                    try
                    {
                        if (string.IsNullOrEmpty(txtYazar.Text))
                        {
                            MessageBox.Show("Yazar bilgisisini giriniz.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Ekleme yapalım:
                            string insertCumlesi = $"INSERT INTO Yazarlar (KayitTarihi, YazarAdSoyad, SilindiMi) VALUES ('{TarihiDüzenle(DateTime.Now)}','{txtYazar.Text.Trim()}',0)";
                            // Tarihi buradan göndermek istemezsek GetDate() ile SQL sorgusu verebiliriz.
                            SqlCommand insertKomut = new SqlCommand(insertCumlesi, baglanti);
                            // Bağlantı sağlayacak metodu çağıralım:
                            BaglantiyiAc();
                            int sonucum = insertKomut.ExecuteNonQuery();
                            if (sonucum > 0) // Affected rows var.
                            {
                                MessageBox.Show("Yeni yazar sisteme eklendi.");
                                TumYazarlariGetir();
                            }
                            else
                            {
                                MessageBox.Show("Bir hata oluştu! Yeni yazark eklenemedi.");
                            }
                            // Bağlantıyı kapatacak metotdu çağıralım:
                            BaglantiyiKapat();

                        }
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Beklenmedik bir hata oluştu! " + ex.Message);
                    }
                    Temizle();
                    break;

                case "GÜNCELLE":
                    try
                    {
                        if (!string.IsNullOrEmpty(txtYazar.Text))
                        {
                            using (baglanti)
                            {
                                DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                                int yazarID = Convert.ToInt32(satir.Cells["YazarID"].Value);

                                // 1. Yol:
                                string updateSorguCumlesi = $"UPDATE Yazarlar SET YazarAdSoyad='{txtYazar.Text.Trim()}' WHERE YazarID={yazarID}";
                                SqlCommand updateKomut = new SqlCommand(updateSorguCumlesi, baglanti);
                                BaglantiyiAc();

                                int sonuc = updateKomut.ExecuteNonQuery();
                                if (sonuc > 0) // Affected rows mevcut ise
                                {
                                    MessageBox.Show($"Yazar güncellendi.");
                                    TumYazarlariGetir();
                                }
                                else
                                {
                                    MessageBox.Show("Yazar güncellenemedi!");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Güncelleştirme yapılamadı!");
                        }



                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show("Beklenmedik bir hata oluştu! " + ex.Message);
                    }
                    Temizle();
                    break;

                default:
                    break;
            }
        }

        private void Temizle()
        {
            btnEkle.Text = "EKLE";
            txtYazar.Clear();
        }

        private void BaglantiyiKapat()
        {
            try
            {
                if (baglanti.State != ConnectionState.Closed)
                {
                    baglanti.Close();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Bağlantı kaoanırken bir hata oluştu!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BaglantiyiAc()
        {
            try
            {
                // Bağlantı açık değilse açalım:
                if (baglanti.State != ConnectionState.Open)
                {
                    baglanti.ConnectionString = SQLBaglantiCumlesi;
                    baglanti.Open();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Bağlantı açılırken bir hata oluştu!", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string TarihiDüzenle(DateTime tarih)
        {
            string tarihString = string.Empty;
            if (tarih != null)
            {
                tarihString = tarih.Year + "-" + tarih.Month + "-" + tarih.Day + " " + tarih.Hour + ":" + tarih.Minute + ":" + tarih.Second;
            }



            return tarihString;
        }

        private void guncelleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewYazarlar.SelectedRows.Count > 0)
            {

                DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                string yazarAdSoyad = Convert.ToString(satir.Cells["YazarAdSoyad"].Value);
                btnEkle.Text = "GÜNCELLE";
                txtYazar.Text = yazarAdSoyad;

                // Kısa yol için:
                // txtYazar.Text = Convert.ToString(satir.Cells["YazarAdSoyad"].Value);
            }
            else
            {
                MessageBox.Show("Güncelleme işlemi için tablodan bir yazar seçmelisiniz!", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow secilenSatir = dataGridViewYazarlar.SelectedRows[0];
            int yazarID = (int)secilenSatir.Cells["YazarID"].Value;
            string yazar = Convert.ToString(secilenSatir.Cells["YazarAdSoyad"].Value);

            /* Yazarın kitapları mevcutsa, Kitaplar tablosunda YazarID Foreign Key vardır.
             * Bu durumda silme işlemi yapılmamalıdır. */

            /* Önce bir SELECT sorgusu ile Kitaplar tablosunda o yazara ait kayıt var mı diye kontrol etmeliyiz.
             * Varsa, silmesine izin vermeyeceğiz.
             * Yoksa silmek ister misin diye son kez sorarak evet derse sileceğiz. */

            SqlCommand komut = new SqlCommand($"SELECT * FROM Kitaplar WHERE YazarID={yazarID}", baglanti); /* SQLCommand nesnesine bağlantısını
                                                                                                             * ve bu bağlantı üzerinde çalıştıracağı sorguyu
                                                                                                             * CTOR'ı üzerinde verdik.*/

            komut.Connection = baglanti;
            SqlDataAdapter adaptor = new SqlDataAdapter(komut); // Adapter'in CTOR'unda işleyeceği komutu verdik.

            DataTable sanalTablo = new DataTable();
            BaglantiyiAc();
            adaptor.Fill(sanalTablo);
            if (sanalTablo.Rows.Count > 0)
            {
                MessageBox.Show($"{yazar} adlı yazarın Kitaplar tablosunda {sanalTablo.Rows.Count.ToString()} adet kitabı bulunmaktadır. Bu yazarı silmek için öncelikle bu yazara tanımlı kitapları silmeniz gerekmektedir. Lütfen Kitap İşlemleri sayfasına gidiniz.");
            }
            else
            {
                // Yazarın kitabı yoksa Foreign Key hatası almayız. O halde yazarı silebiliriz.
                DialogResult cevap = MessageBox.Show($"{yazar} adlı yazar, Yazarlar tablosundan silinsin mi?", "ONAY", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (cevap == DialogResult.Yes)
                {
                    // Cevap "Evet" ise silinsin.

                    //// 1. yol:
                    //komut.CommandText = $"DELETE FROM Yazarlar WHERE YazarID={yazarID}";

                    // 2. yol:
                    komut.CommandText = $"DELETE FROM Yazarlar WHERE YazarID=@yzrid";
                    komut.Parameters.Clear();
                    komut.Parameters.AddWithValue("@yzrid", yazarID); // AddWithValue metodu SQLCommand nesnesinin commandText'inde bulunan sorgu cümlesinde, @yzrid yerine yazarID değerini entegre eder.
                    BaglantiyiAc();
                    int sonuc = komut.ExecuteNonQuery();
                    if (sonuc > 0)
                    {
                        MessageBox.Show("Silindi");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("HATA! Silinemedi.");
                    }
                    BaglantiyiKapat();
                }
            }
        }

        private void silPasifeCekToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Kullanıcı silindiğini sanacak, fakat pasife alınacak.
            try
            {
                using (baglanti)
                {
                    DataGridViewRow satir = dataGridViewYazarlar.SelectedRows[0];
                    int yazarID = Convert.ToInt32(satir.Cells["YazarID"].Value);

                    // 1. Yol:
                    //string updateSorguCumlesi = $"UPDATE Yazarlar SET SilindiMi=1 WHERE YazarID={yazarID}";
                    //SqlCommand updateKomut = new SqlCommand(updateSorguCumlesi, baglanti);
                    // 2. yol: AddWtihValue
                    string updateSorguCumlesi = $"UPDATE Yazarlar SET SilindiMi=1 WHERE YazarID=@yzrid";
                    SqlCommand updateCommand = new SqlCommand(updateSorguCumlesi, baglanti);
                    updateCommand.Parameters.Clear();
                    updateCommand.Parameters.AddWithValue("@yzrid", yazarID);


                    BaglantiyiAc();



                    int sonuc = updateCommand.ExecuteNonQuery();
                    if (sonuc > 0) // Affected rows mevcut ise
                    {
                        MessageBox.Show($"Yazar silindi.");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("Yazar silinemedi!");
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("Silme (Pasife çek) işleminde hata! " + ex.Message);
            }

        }

        private void silBaskaBirYontemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Bu yöntem yukarıdakiler gibi kullanışlı değildir.
            try
            {

                DataGridViewRow secilenSatir = dataGridViewYazarlar.SelectedRows[0];
                int yazarId = (int)secilenSatir.Cells["YazarId"].Value;
                string yazar = Convert.ToString(secilenSatir.Cells["YazarAdSoyad"].Value);
                SqlCommand komut = new SqlCommand();
                komut.Connection = baglanti;

                DialogResult cevap = MessageBox.Show($"{yazar} adlı yazarı silmek istediğinize emin misiniz?", "ONAY", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (cevap == DialogResult.Yes)
                {
                    //silecek
                    //komut.CommandText = $"Delete from Yazarlar where YazarId={yazarId}";
                    //@yzrid diyerek bir parametre oluşturmuş olduk.
                    komut.CommandText = $"Delete from Yazarlar where YazarId=@yzrid";
                    komut.Parameters.Clear();
                    //AddWithValue metodu @yzrid yerine yazarId değerini sqlcommand nesnesinin commendText'inde bulunan sorgu cümlesine entegre eder.
                    komut.Parameters.AddWithValue("@yzrid", yazarId);

                    BaglantiyiAc();
                    int sonuc = komut.ExecuteNonQuery();
                    if (sonuc > 0)
                    {
                        MessageBox.Show("Silindi");
                        TumYazarlariGetir();
                    }
                    else
                    {
                        MessageBox.Show("HATA:Silinemedi!");
                    }
                    BaglantiyiKapat();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("HATA: " + ex.Message);
            }
        }


    }
}
