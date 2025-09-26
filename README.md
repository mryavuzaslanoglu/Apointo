# Apointo

Kuaför & Berber randevu sistemi projesi. 

## Baþlangýç
- .NET 9 SDK gerekir.
- Ayrýntýlý konfigürasyon adýmlarý için [docs/configuration.md](docs/configuration.md) dosyasýna bakýn.
- Çalýþtýrmadan önce `dotnet ef database update` komutuyla veri tabanýný güncelleyin.

## Docker
```
cp .env.docker.example .env.docker
# deðerleri düzenleyin
docker-compose up --build
```

## Modül Durumu
- Modül 1 Backend altyapýsý tamamlandý, testler ve frontend/mobil parçalarý sýrada.