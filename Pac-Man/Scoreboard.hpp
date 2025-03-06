#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <algorithm>
#include <filesystem>

class Scoreboard {
public:
    Scoreboard(float width, float height) {
        if (!font.loadFromFile("arial.ttf")) {
            std::cerr << "Failed to load font!" << std::endl;
        }
        title.setFont(font);
        title.setFillColor(sf::Color::Yellow);
        title.setString("Scoreboards");
        title.setPosition(width / 2.f - title.getGlobalBounds().width / 2.f, height / 8.f);

        loadScores("scores.txt");
    }

    bool loadScores(const std::string& filename) {
        std::filesystem::path filePath = filename;
        std::ifstream file(filePath);
        if (!file.is_open()) {
            return false;
        }
        scores.clear();
        std::string line;
        while (std::getline(file, line)) {
            scores.push_back(line);
        }
        file.close();

        std::sort(scores.begin(), scores.end(), [](const std::string& a, const std::string& b) {
            int scoreA = std::stoi(a.substr(a.find_last_of(' ') + 1));
            int scoreB = std::stoi(b.substr(b.find_last_of(' ') + 1));
            return scoreA > scoreB;
            });

        updateText();
        return true;
    }

    void draw(sf::RenderWindow& window) {
        window.draw(title);
        for (auto& s : scoreTexts) {
            window.draw(s);
        }
    }

private:
    void updateText() {
        scoreTexts.clear();
        for (size_t i = 0; i < scores.size(); i++) {
            sf::Text text;
            text.setFont(font);
            text.setFillColor(sf::Color::Red);
            text.setString(scores[i]);
            text.setCharacterSize(40);
            text.setPosition(400.f, 150.f + i * 50.f);
            scoreTexts.push_back(text);
        }
    }

    std::vector<std::string> scores;
    std::vector<sf::Text> scoreTexts;
    sf::Font font;
    sf::Text title;
};
