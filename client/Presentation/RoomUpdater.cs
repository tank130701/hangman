using client.Domain.Interfaces;
using client.Menu;

public class RoomUpdater
{
    private readonly IGameDriver _gameDriver;
    public RoomUpdater(IGameDriver gameDriver)
    {
        _gameDriver = gameDriver;
    }
    // Выбор категории
    private string? ChooseCategory()
    {
        ConsoleMenu categoryMenu = new ConsoleMenu("==>");
        categoryMenu.Header = "=== Choose a Category ===";

        string? selectedCategory = null; // Изначально значение не выбрано

        // Добавляем пункты меню
        categoryMenu.addMenuItem(1, "Животные", () => { selectedCategory = "животные"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(2, "Фрукты", () => { selectedCategory = "фрукты"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(3, "Столицы", () => { selectedCategory = "столицы"; categoryMenu.hideMenu(); });
        categoryMenu.addMenuItem(0, "Back", () => { selectedCategory = null; categoryMenu.hideMenu(); });

        // Показываем меню
        categoryMenu.showMenu();
        return selectedCategory;
    }

    // Выбор сложности
    private string? ChooseDifficulty()
    {
        ConsoleMenu difficultyMenu = new ConsoleMenu("==>");
        difficultyMenu.Header = "=== Choose Difficulty ===";

        string? selectedDifficulty = null; // Изначально значение не выбрано

        // Добавляем пункты меню
        difficultyMenu.addMenuItem(1, "Easy", () => { selectedDifficulty = "easy"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(2, "Medium", () => { selectedDifficulty = "medium"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(3, "Hard", () => { selectedDifficulty = "hard"; difficultyMenu.hideMenu(); });
        difficultyMenu.addMenuItem(0, "Back", () => { selectedDifficulty = null; difficultyMenu.hideMenu(); });

        // Показываем меню
        difficultyMenu.showMenu();
        return selectedDifficulty;
    }
    public void ChangeCategory(string roomId, string roomPassword)
    {
        Console.WriteLine("Changing Category...");
        var newCategory = ChooseCategory();
        if (newCategory != null)
        {
            try
            {
                var response = _gameDriver.UpdateRoom(roomId, roomPassword, newCategory, null, null);
                if (response.Message != null)
                    Console.WriteLine("Category updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update category: {ex.Message}");
                return;
            }
        }
        else
        {
            Console.WriteLine("Category update canceled.");
        }
    }

    public void ChangeDifficulty(string roomId, string roomPassword)
    {
        Console.WriteLine("Changing Difficulty...");
        var newDifficulty = ChooseDifficulty();
        if (newDifficulty != null)
        {
            try
            {
                var response = _gameDriver.UpdateRoom(roomId, roomPassword, null, newDifficulty, null);
                if (response.Message != null)
                    Console.WriteLine("Difficulty updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update difficulty: {ex.Message}");
                return;
            }
        }
        else
        {
            Console.WriteLine("Difficulty update canceled.");
        }
    }

    public void ChangePassword(string roomId, string roomPassword)
    {
        Console.Write("Enter new password: ");
        var newPassword = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newPassword))
        {
            try
            {
                var response = _gameDriver.UpdateRoom(roomId, roomPassword, null, null, newPassword);
                if (response.Message != null)
                    Console.WriteLine("Password updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update password: {ex.Message}");
                return;
            }
        }
        else
        {
            Console.WriteLine("Password cannot be empty. Update canceled.");
        }
    }

}