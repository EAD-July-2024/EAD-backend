/*
 * File: FirebaseService.cs
 * Author: [​Thilakarathne S.P. ]

 * Description: 
 *     This file contains the FirebaseService class, which is responsible for 
 *     initializing the Firebase Admin SDK and sending push notifications to devices 
 *     using Firebase Cloud Messaging (FCM). The service includes methods for sending 
 *     notifications to individual devices and to multiple devices based on their FCM tokens.
 * 
 * Dependencies:
 *     - FirebaseAdmin: The Firebase Admin SDK for .NET.
 *     - Google.Apis.Auth.OAuth2: For handling Google credentials and authentication.
 * 
 * Methods:
 *     - FirebaseService: Constructor that initializes the Firebase service by calling 
 *       the InitializeFirebase method.
 *     - InitializeFirebase: 
 *         Checks if the FirebaseApp instance is null; if so, it initializes it using 
 *         the provided service account credentials from a JSON file.
 * 
 *     - SendNotificationAsync: 
 *         Sends a notification to a device specified by the FCM token. The method 
 *         constructs a Message object with the provided title and body, then sends 
 *         the message asynchronously. It logs the response from the Firebase server.
 * 
 *     - SendNotificationToCsrAsync: 
 *         Sends notifications to multiple devices by iterating through a list of FCM 
 *         tokens and calling the SendNotificationAsync method for each token. It logs 
 *         a message for each token processed.
 * 

 */

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
