using Microsoft.EntityFrameworkCore;  
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Contacts.Pages
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public partial class MainPage : Page
    {
        private ContactsDbContext _dbContext;
        private ObservableCollection<Contact> _contacts;

        public MainPage()
        {
            InitializeComponent();
            _dbContext = new ContactsDbContext(new DbContextOptions<ContactsDbContext>());
            _contacts = new ObservableCollection<Contact>();
            ContactsDataGrid.ItemsSource = _contacts;

            Loaded += async (s, e) => await LoadContactsAsync();
        }

        private async Task LoadContactsAsync()
        {
            var contacts = await _dbContext.Contacts.ToListAsync(); // Получаем все контакты из базы данных
            _contacts.Clear();
            foreach (var contact in contacts)
            {
                _contacts.Add(contact);
            }
        }

        private async void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            var newContact = new Contact { Name = "Новый контакт", Phone = "1234567890", Address = "Адрес" };
            await _dbContext.Contacts.AddAsync(newContact); // Добавляем новый контакт в базу данных
            await _dbContext.SaveChangesAsync(); // Сохраняем изменения

            _contacts.Add(newContact); // Добавляем контакт в коллекцию
        }

        private async void DeleteContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactsDataGrid.SelectedItem is Contact selectedContact)
            {
                _dbContext.Contacts.Remove(selectedContact); // Удаляем контакт из базы данных
                await _dbContext.SaveChangesAsync(); // Сохраняем изменения

                _contacts.Remove(selectedContact); // Удаляем контакт из коллекции
            }
        }

        private async void EditContactButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContactsDataGrid.SelectedItem is Contact selectedContact)
            {
                selectedContact.Name = "Измененное имя"; // Изменяем имя контакта
                _dbContext.Contacts.Update(selectedContact); // Обновляем контакт в базе данных
                await _dbContext.SaveChangesAsync(); // Сохраняем изменения

                // Обновляем контакт в коллекции
                var index = _contacts.IndexOf(selectedContact);
                _contacts[index] = selectedContact;
            }
        }

        private async void ApplyFilterAndSort()
        {
            var query = _dbContext.Contacts.AsQueryable();

            string filterText = FilterTextBox.Text;
            if (!string.IsNullOrEmpty(filterText))
            {
                switch (FilterComboBox.SelectedIndex)
                {
                    case 0: // Фильтр по имени
                        query = query.Where(c => EntityFrameworkCore.Functions.Like(c.Name, $"%{filterText}%"));
                        break;
                    case 1: // Фильтр по телефону
                        query = query.Where(c => EntityFrameworkCore.Functions.Like(c.Phone, $"%{filterText}%"));
                        break;
                    case 2: // Фильтр по адресу
                        query = query.Where(c => EntityFrameworkCore.Functions.Like(c.Address, $"%{filterText}%"));
                        break;
                }
            }

            switch (SortComboBox.SelectedIndex)
            {
                case 0: // Сортировка по имени
                    query = query.OrderBy(c => c.Name);
                    break;
                case 1: // Сортировка по телефону
                    query = query.OrderBy(c => c.Phone);
                    break;
                case 2: // Сортировка по адресу
                    query = query.OrderBy(c => c.Address);
                    break;
            }

            var contacts = await query.ToListAsync(); // Получаем контакты из базы данных с фильтрацией и сортировкой

            _contacts.Clear();
            foreach (var contact in contacts)
            {
                _contacts.Add(contact);
            }
        }
    }
}
