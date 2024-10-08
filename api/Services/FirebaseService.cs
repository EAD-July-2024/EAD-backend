using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

public class FirebaseService
{
    private static FirebaseApp? _firebaseApp;

    public FirebaseService()
    {
        // InitializeFirebase();
    }

    // private void InitializeFirebase()
    // {
    //     if (_firebaseApp == null)
    //     {
    //         _firebaseApp = FirebaseApp.Create(new AppOptions()
    //         {
    //             Credential = GoogleCredential.FromFile("/Users/SLIIT/Year 04/EAD/Assignment/Work/Repos/Backend/EAD-backend/api/ead-e-commerce-ee253-firebase-adminsdk-6aity-28ec1334ed.json")
    //         });
    //     }
    // }

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

    // Method to send notifications to multiple tokens
    public async Task SendNotificationToCsrAsync(List<string> fcmTokens, string title, string body)
    {
        foreach (var token in fcmTokens)
        {
            await SendNotificationAsync(token, title, body);  // Assuming you have a method to send individual notifications
            Console.WriteLine("This hit ");
        }
    }
}
