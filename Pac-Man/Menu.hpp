#pragma once
#include <SFML/Graphics.hpp> 
#include <vector> 
#include <string> 

// Klasa Menu zarz�dza menu g��wnym gry
class Menu {
public:
    // Konstruktor klasy Menu, inicjalizuje r�ne zmienne i wczytuje zasoby
    Menu(float width, float height) {
        if (!font.loadFromFile("arial.ttf")) { // Pr�buje za�adowa� czcionk� z pliku
            // handle error
        }

        // Menu options - Opcje menu
        menuOptions = { "Start", "Scoreboards", "Options", "Exit" }; // Lista opcji menu

        // Tworzy teksty dla ka�dej opcji menu
        for (size_t i = 0; i < menuOptions.size(); ++i) {
            sf::Text text; // Tworzy nowy obiekt tekstu
            text.setFont(font); // Ustawia czcionk� dla tekstu
            text.setFillColor(sf::Color::Yellow); // Ustawia kolor tekstu na ��ty
            text.setString(menuOptions[i]); // Ustawia tekst na odpowiedni� opcj� menu
            text.setCharacterSize(50); // Ustawia rozmiar czcionki
            text.setPosition(sf::Vector2f(width / 2 - text.getGlobalBounds().width / 2, height / (menuOptions.size() + 1) * (i + 1))); // Ustawia pozycj� tekstu
            menuItems.push_back(text); // Dodaje tekst do wektora menuItems
        }
        selectedItemIndex = 0; // Ustawia indeks wybranej opcji na 0
        menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor wybranej opcji na ��ty
    }

    // Funkcja rysuj�ca menu na oknie
    void draw(sf::RenderWindow& window) {
        for (const auto& item : menuItems) { // Iteruje po wszystkich opcjach menu
            window.draw(item); // Rysuje ka�d� opcj� menu na oknie
        }
    }

    // Funkcja poruszaj�ca wyb�r w menu w g�r�
    void moveUp() {
        if (selectedItemIndex - 1 >= 0) { // Sprawdza, czy mo�na przesun�� wyb�r w g�r�
            menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor bie��cej opcji na ��ty
            selectedItemIndex--; // Zmniejsza indeks wybranej opcji
            menuItems[selectedItemIndex].setFillColor(sf::Color::Red); // Ustawia kolor nowej wybranej opcji na czerwony
        }
    }

    // Funkcja poruszaj�ca wyb�r w menu w d�
    void moveDown() {
        if (selectedItemIndex + 1 < menuItems.size()) { // Sprawdza, czy mo�na przesun�� wyb�r w d�
            menuItems[selectedItemIndex].setFillColor(sf::Color::Yellow); // Ustawia kolor bie��cej opcji na ��ty
            selectedItemIndex++; // Zwi�ksza indeks wybranej opcji
            menuItems[selectedItemIndex].setFillColor(sf::Color::Red); // Ustawia kolor nowej wybranej opcji na czerwony
        }
    }

    // Funkcja zwracaj�ca indeks wybranej opcji
    int getSelectedItemIndex() {
        return selectedItemIndex; // Zwraca indeks wybranej opcji
    }

private:
    std::vector<std::string> menuOptions; // Wektor string�w przechowuj�cy opcje menu
    std::vector<sf::Text> menuItems; // Wektor tekst�w przechowuj�cy opcje menu jako obiekty sf::Text
    sf::Font font; // Czcionka u�ywana w menu
    int selectedItemIndex; // Indeks wybranej opcji
};
