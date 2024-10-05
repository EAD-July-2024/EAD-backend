using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

public class FirebaseService
{
    private static FirebaseApp _firebaseApp;

    public FirebaseService()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        if (_firebaseApp == null)
        {
            _firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("C:/Users/sahanp/Desktop/Change/EAD-backend/api/ead-e-commerce-ee253-firebase-adminsdk-6aity-a0398f8b4a.json") // Ensure this is the correct path.
            });
        }
    }

    // Method to send notification
    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var message = new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        // Send a message to the device corresponding to the provided token.
        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine("Successfully sent message: " + response);
    }
}
