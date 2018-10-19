# GoogleTasksSynchronizer
Azure Function to Synchronize Google Tasks between multiple accounts.

## Deployment

To set up the application create a Google Developer API account here: https://console.developers.google.com/apis/.

Add the API Key to your configuration either through local.settings.json or through environment variables.  The key should be "GoogleClientSecret".  The value should be the json provided when you "Download JSON" from Google's Credentials screen.

Google Account credentials also need to be provided in the same way.  The key for this should be "TaskAccounts" and the value should be JSON like the following:

[
  {
    "AccountName": "GOOGLE ACCOUNT 1",
    "TaskListId": "Task List Id"
  },
  {
    "AccountName": "GOOGLE ACCOUNT 2",
    "TaskListId": "Task List Id"
  }
]

The first time the application runs you'll need to enable access to your Google API Account from your Google Account with OAuth.  This will require a browser and can't occur in Azure so you must run the function locally.

Once this is complete the application will synchronize your Google Tasks whenever it runs.
