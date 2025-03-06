#pragma once
#include <SFML/Graphics.hpp> 
#include <vector> 
#include <string> 

// Klasa Menu zarz¹dza menu g³ównym gry
class Menu {
public:
    // Konstruktor klasy Menu, inicjalizuje ró¿ne zmienne i wczytuje zasoby
    Menu(float width, float height) {
        if (!font.loadFromFile("arial.ttf")) { // Próbuje za³adowaæ czcionkê z pliku
            // handle error
        }

        // Menu options - Opcje menu
        menuOptions = { "Start", "Scoreboards", "Options", "Exit" }; // Lista opcji menu

        // Tworzy teksty dla ka¿dej opcji menu
        for (size_t i = 0; i < menuOptions.size(); ++i) {
            sf::Text text; // Tworzy nowy obiekt tekstu
            text.setFont(font); // Ustawia czcionkê dla tekstu
            text.setFillColor(sf::Color::Yellow); // Ustawia kolor tekstu na ¿ó³ty
            text.setString(menuOptions[i]); // Ustawia tekst na odpowiedni¹ opcjê menu
            text.setCharacterSize(50); // Ustawia rozmiar czcionki
            text.setPosition(sf::Vector2f(width / 2 - text.getGlobalBounds().width / 2, height / (menuOptions.size() + 1) * (i + 1))); // Ustawia pozycjê tekstu
            menuItems.push_back(text); // Dodaje tekst do wektora menuItems
        }
        selectedItemIndex = 0; // Ustawia indeks wybranej opcji na 0
        menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor wybranej opcji na ¿ó³ty
    }

    // Funkcja rysuj¹ca menu na oknie
    void draw(sf::RenderWindow& window) {
        for (const auto& item : menuItems) { // Iteruje po wszystkich opcjach menu
            window.draw(item); // Rysuje ka¿d¹ opcjê menu na oknie
        }
    }

    // Funkcja poruszaj¹ca wybór w menu w górê
    void moveUp() {
        if (selectedItemIndex - 1 >= 0) { // Sprawdza, czy mo¿na przesun¹æ wybór w górê
            menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor bie¿¹cej opcji na ¿ó³ty
            selectedItemIndex--; // Zmniejsza indeks wybranej opcji
            menuItems[selectedItemIndex].setFillColor(sf::Color::Red); // Ustawia kolor nowej wybranej opcji na czerwony
        }
    }

    // Funkcja poruszaj¹ca wybór w menu w dó³
    void moveDown() {
        if (selectedItemIndex + 1 < menuItems.size()) { // Sprawdza, czy mo¿na przesun¹æ wybór w dó³
            menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor bie¿¹cej opcji na ¿ó³ty
            selectedItemIndex++; // Zwiêksza indeks wybranej opcji
            menuItems[selectedItemIndex].setFillColor(sf::Color::Red); // Ustawia kolor nowej wybranej opcji na czerwony
        }
    }

    // Funkcja zwracaj¹ca indeks wybranej opcji
    int getSelectedItemIndex() {
        return selectedItemIndex; // Zwraca indeks wybranej opcji
    }

private:
    std::vector<std::string> menuOptions; // Wektor stringów przechowuj¹cy opcje menu
    std::vector<sf::Text> menuItems; // Wektor tekstów przechowuj¹cy opcje menu jako obiekty sf::Text
    sf::Font font; // Czcionka u¿ywana w menu
    int selectedItemIndex; // Indeks wybranej opcji
};
