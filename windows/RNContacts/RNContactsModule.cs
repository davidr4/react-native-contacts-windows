using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Popups;

namespace Com.Reactlibrary.RNContacts
{
    /// <summary>
    /// A module that allows JS to share data.
    /// </summary>
    class RNContactsModule : ReactContextNativeModuleBase
    {
        /// <summary>
        /// Instantiates the <see cref="RNContactsModule"/>.
        /// </summary>
        public RNContactsModule(ReactContext reactContext) : base(reactContext)
        {

        }

        /// <summary>
        /// The name of the native module.
        /// </summary>
        public override string Name
        {
            get
            {
                return "RNContacts";
            }
        }

        [ReactMethod]
        public async void addContact(JObject config)
        {
            string successMessage = config.Value<string>("successMessage");
            string errorMessage = config.Value<string>("errorMessage");
            
            if (await saveContactToAddressBook(config))
            {
                var messageDialog = new MessageDialog(successMessage);
                await messageDialog.ShowAsync();
            }
            else
            {
                var messageDialog = new MessageDialog(errorMessage);
                await messageDialog.ShowAsync();
            }
            
        }

        private async Task<bool> saveContactToAddressBook(JObject c)
        {
            ContactStore store = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
            string defaultListName = "myPrivateList";
            /* Search for lists in contact list */
            var lists = await store.FindContactListsAsync();
            ContactList list = lists.FirstOrDefault((x) => x.DisplayName == "myPrivateList");
            if (list == null)
            {
                list = await store.CreateContactListAsync(defaultListName);
                list.OtherAppReadAccess = ContactListOtherAppReadAccess.Full;
                list.OtherAppWriteAccess = ContactListOtherAppWriteAccess.SystemOnly;
                await list.SaveAsync();

            }

            if (c["name"] != null)
            {
                Contact contact = new Contact();
                contact.Name = c.Value<string>("name");
                if (c["email"] != null)
                {
                    contact.Emails.Add(new ContactEmail() { Kind = ContactEmailKind.Work, Address = c.Value<string>("email") });
                }
                if (c["phone"] != null)
                {
                    contact.Phones.Add(new ContactPhone() { Kind = ContactPhoneKind.Mobile, Number = c.Value<string>("phone") });
                }
                await list.SaveContactAsync(contact);
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}
