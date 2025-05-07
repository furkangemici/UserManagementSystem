# UserManagementSystem

## Proje Hakkında

Bu proje, kullanıcı yönetimi için geliştirilmiş bir Windows Forms uygulamasıdır. Kullanıcıların eklenmesi, güncellenmesi, silinmesi ve e-posta ile bilgilendirilmesi gibi temel işlevleri içerir.

## Özellikler

- Kullanıcı ekleme, güncelleme ve silme
- Kullanıcı bilgilerini Excel dosyasına aktarma
- E-posta ile kullanıcı şifre bilgilendirmesi
- Kullanıcı bilgilerini doğrulama (e-posta, telefon numarası)

## Gereksinimler

- .NET Framework 4.7.2
- Visual Studio 2019 veya daha yeni bir sürüm
- Gmail hesabı ve uygulama şifresi (e-posta gönderimi için)

## Kurulum

1. Projeyi klonlayın:
   ```bash
   git clone https://github.com/furkangemici/UserManagementSystem.git
   ```

2. Projeyi Visual Studio'da açın.

3. `App.config` dosyasını düzenleyerek kendi Gmail hesabınızı ve uygulama şifrenizi ekleyin:
   ```xml
   <appSettings>
     <add key="MailAddress" value="your-email@gmail.com" />
     <add key="MailPassword" value="your-app-password" />
   </appSettings>
   ```

4. Projeyi derleyin ve çalıştırın.

## Kullanım

- Uygulama başlatıldığında, kullanıcı ekleme, güncelleme ve silme işlemlerini gerçekleştirebilirsiniz.
- Kullanıcı bilgilerini Excel dosyasına aktarmak için "Export" butonunu kullanabilirsiniz.
- Kullanıcı şifrelerini e-posta ile göndermek için "Send Mail" butonunu kullanabilirsiniz.

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.

## İletişim

Sorularınız veya önerileriniz için [GitHub](https://github.com/furkangemici) üzerinden iletişime geçebilirsiniz.
