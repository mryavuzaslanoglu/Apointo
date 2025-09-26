# Apointo

Kuaf�r & Berber randevu sistemi projesi. 

## Ba�lang��
- .NET 9 SDK gerekir.
- Ayr�nt�l� konfig�rasyon ad�mlar� i�in [docs/configuration.md](docs/configuration.md) dosyas�na bak�n.
- �al��t�rmadan �nce `dotnet ef database update` komutuyla veri taban�n� g�ncelleyin.

## Docker
```
cp .env.docker.example .env.docker
# de�erleri d�zenleyin
docker-compose up --build
```

## Mod�l Durumu
- Mod�l 1 Backend altyap�s� tamamland�, testler ve frontend/mobil par�alar� s�rada.