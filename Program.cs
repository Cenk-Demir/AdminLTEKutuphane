using AdminLTEKutuphane.Services;  // FirestoreService sınıfını kullanabilmek için ekledik

public class Program
{
    public static void Main(string[] args)
    {
        // WebApplication builder'ı oluşturuyoruz.
        var builder = WebApplication.CreateBuilder(args);

        // FirestoreService sınıfını DI ile ekliyoruz
        builder.Services.AddSingleton<FirestoreService>();

        // Uygulama oluşturuluyor
        var app = builder.Build();

        // Basit bir test endpoint'i
        app.MapGet("/", () => "Hello Firestore!");

        // Firestore ile veri eklemek için bir test route'u ekliyoruz
        app.MapPost("/add-to-firestore", async (FirestoreService firestoreService) =>
        {
            // Firestore'a veri eklemek için örnek veri
            var data = new Dictionary<string, object>
            {
                { "name", "John Doe" },
                { "email", "john.doe@example.com" },
                { "createdAt", DateTime.UtcNow }
            };

            // FirestoreService kullanarak veriyi Firestore'a ekliyoruz
            await firestoreService.AddDocumentAsync("users", "user1", data);
            return Results.Ok("Document added successfully!");
        });

        // Firestore'dan veri almak için bir test route'u ekliyoruz
        app.MapGet("/get-from-firestore/{id}", async (string id, FirestoreService firestoreService) =>
        {
            var doc = await firestoreService.GetDocumentAsync("users", id);
            if (doc.Exists)
            {
                return Results.Ok(doc.ToDictionary());
            }
            return Results.NotFound("Document not found.");
        });

        // Uygulama başlatılıyor
        app.Run();
    }
}
