#pragma once
#include <SFML/Graphics.hpp>
#include <SFML/Audio.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <algorithm>

class OptionsMenu {
public:
    OptionsMenu(float width, float height) {
        if (!font.loadFromFile("arial.ttf")) {
            std::cerr << "Failed to load font!" << std::endl;
        }
        title.setFont(font);
        title.setFillColor(sf::Color::Yellow);
        title.setString("Options");
        title.setCharacterSize(70);
        title.setPosition(width / 2.f - title.getGlobalBounds().width / 2.f, height / 12.f);

        setupOptions(width, height);
        currentSectionIndex = 0;
        currentItemIndex = 0;
        updateSelection();

        selectedBackground = "thegaijinos.png";

        if (!music.openFromFile("PacMan Original Theme.wav")) {
            std::cerr << "Failed to load PacMan Original Theme.wav" << std::endl;
        }
        else {
            music.setLoop(true);
            music.play();
        }
    }

    void setupOptions(float width, float height) {
        optionsSections = {
            {"Map Selection:", {"Hills", "Woods", "Dungeon"}},
            {"Menu Background:", {"Gaijinos", "The Snail", "LettuceEater"}},
            {"Music Selection:", {"BF4", "Pac-man", "Levrone"}}
        };

        float sectionY = height / 6.f;
        for (auto& section : optionsSections) {
            sf::Text sectionTitle;
            sectionTitle.setFont(font);
            sectionTitle.setFillColor(sf::Color::Red);
            sectionTitle.setString(section.first);
            sectionTitle.setCharacterSize(50);
            sectionTitle.setPosition(100.f, sectionY);
            optionTitles.push_back(sectionTitle);

            float optionX = 150.f;
            std::vector<sf::Text> sectionItems;
            for (auto& opt : section.second) {
                sf::Text optionText;
                optionText.setFont(font);
                optionText.setFillColor(sf::Color::White);
                optionText.setString(opt);
                optionText.setCharacterSize(40);
                optionText.setPosition(optionX, sectionY + 60.f);
                sectionItems.push_back(optionText);
                optionX += 200.f;
            }
            optionItems.push_back(sectionItems);
            sectionY += 200.f;
        }
    }

    void draw(sf::RenderWindow& window) {
        window.draw(title);
        for (auto& t : optionTitles) {
            window.draw(t);
        }
        for (auto& section : optionItems) {
            for (auto& item : section) {
                window.draw(item);
            }
        }
    }

    void moveUp() {
        if (currentSectionIndex > 0) {
            currentSectionIndex--;
            currentItemIndex = 0;
            updateSelection();
        }
    }

    void moveDown() {
        if (currentSectionIndex < optionsSections.size() - 1) {
            currentSectionIndex++;
            currentItemIndex = 0;
            updateSelection();
        }
    }

    void moveLeft() {
        if (currentItemIndex > 0) {
            currentItemIndex--;
            updateSelection();
        }
    }

    void moveRight() {
        if (currentItemIndex < optionItems[currentSectionIndex].size() - 1) {
            currentItemIndex++;
            updateSelection();
        }
    }

    std::pair<int, int> getSelectedItem() {
        return { currentSectionIndex, currentItemIndex };
    }

    void playSelectedMusic() {
        int section = currentSectionIndex;
        int item = currentItemIndex;
        if (section == 2) { // Music
            music.stop();
            bool loaded = false;
            std::string musicFile;
            if (item == 0) {
                musicFile = "Battlefield 4 Warsaw Theme.wav";
            }
            else if (item == 1) {
                musicFile = "PacMan Original Theme.wav";
            }
            else {
                musicFile = "Levrone.wav";
            }
            loaded = music.openFromFile(musicFile);
            if (!loaded) {
                std::cerr << "Failed to load " << musicFile << std::endl;
            }
            else {
                music.setLoop(true);
                music.play();
            }
        }
    }

    std::string getSelectedBackground() {
        return selectedBackground;
    }

    std::string getSelectedMap() {
        int section = currentSectionIndex;
        int item = currentItemIndex;
        // Pierwsza sekcja: mapy
        if (section == 0) {
            switch (item) {
            case 0: return "map1.txt";
            case 1: return "map2.txt";
            case 2: return "map3.txt";
            }
        }
        return "map1.txt";
    }

private:
    void updateSelection() {
        for (size_t i = 0; i < optionItems.size(); i++) {
            for (size_t j = 0; j < optionItems[i].size(); j++) {
                if (i == currentSectionIndex && j == currentItemIndex) {
                    optionItems[i][j].setFillColor(sf::Color::Red);
                    if (i == 1) {
                        switch (j) {
                        case 0: selectedBackground = "thegaijinos.png"; break;
                        case 1: selectedBackground = "The Snail.png";   break;
                        case 2: selectedBackground = "LettuceEater.png"; break;
                        }
                    }
                }
                else {
                    optionItems[i][j].setFillColor(sf::Color::White);
                }
            }
        }
    }

    sf::Font font;
    sf::Text title;
    std::vector<std::pair<std::string, std::vector<std::string>>> optionsSections;
    std::vector<sf::Text> optionTitles;
    std::vector<std::vector<sf::Text>> optionItems;
    int currentSectionIndex;
    int currentItemIndex;

    sf::Music music;
    std::string selectedBackground;
};
