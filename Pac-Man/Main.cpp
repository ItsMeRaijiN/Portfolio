#include <SFML/Graphics.hpp>
#include <iostream>
#include <regex>
#include "Menu.hpp"
#include "Scoreboard.hpp"
#include "Options.hpp"
#include "Game.hpp"

int main() {
    sf::RenderWindow window(sf::VideoMode(1200, 800), "Pac-Man");

    sf::Texture backgroundTexture;
    if (!backgroundTexture.loadFromFile("thegaijinos.png")) {
        std::cerr << "Failed to load background texture!" << std::endl;
        return -1;
    }
    sf::Sprite backgroundSprite;
    backgroundSprite.setTexture(backgroundTexture);

    Menu menu(window.getSize().x, window.getSize().y);
    Scoreboard scoreboard(window.getSize().x, window.getSize().y);
    OptionsMenu optionsMenu(window.getSize().x, window.getSize().y);
    Game game(window.getSize().x, window.getSize().y);

    bool isScoreboard = false;
    bool isOptions = false;
    bool isGame = false;
    bool showRules = false;
    bool isGameRunning = false;
    bool enterName = false;

    std::string playerName;
    sf::Font font;
    if (!font.loadFromFile("arial.ttf")) {
        std::cerr << "Failed to load font!" << std::endl;
        return -1;
    }

    sf::Text nameText;
    nameText.setFont(font);
    nameText.setFillColor(sf::Color::Yellow);
    nameText.setCharacterSize(50);
    nameText.setPosition(400, 300);

    sf::Clock clock;

    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) {
                window.close();
            }
            if (event.type == sf::Event::TextEntered && enterName) {
                if (event.text.unicode == '\b') {
                    if (!playerName.empty()) {
                        playerName.pop_back();
                    }
                }
                else if (event.text.unicode == '\r') {
                    if (std::regex_match(playerName, std::regex("^[a-zA-Z0-9]{3,15}$"))) {
                        enterName = false;
                        showRules = true;
                        game.setPlayerName(playerName);
                    }
                    else {
                        playerName.clear();
                    }
                }
                else if (event.text.unicode < 128) {
                    playerName += static_cast<char>(event.text.unicode);
                }
                nameText.setString("Enter your name:\n" + playerName);
            }
            if (event.type == sf::Event::KeyPressed) {
                if (event.key.code == sf::Keyboard::Up) {
                    if (!isScoreboard && !isOptions && !showRules && !isGame && !enterName) {
                        menu.moveUp();
                    }
                    else if (isOptions) {
                        optionsMenu.moveUp();
                    }
                }
                if (event.key.code == sf::Keyboard::Down) {
                    if (!isScoreboard && !isOptions && !showRules && !isGame && !enterName) {
                        menu.moveDown();
                    }
                    else if (isOptions) {
                        optionsMenu.moveDown();
                    }
                }
                if (event.key.code == sf::Keyboard::Left) {
                    if (isOptions) {
                        optionsMenu.moveLeft();
                    }
                }
                if (event.key.code == sf::Keyboard::Right) {
                    if (isOptions) {
                        optionsMenu.moveRight();
                    }
                }
                if (event.key.code == sf::Keyboard::Return) {
                    if (!isScoreboard && !isOptions && !showRules && !isGame && !enterName) {
                        int selectedItem = menu.getSelectedItemIndex();
                        if (selectedItem == 0) {
                            enterName = true;
                            playerName.clear();
                        }
                        else if (selectedItem == 1) {
                            isScoreboard = true;
                            isOptions = false;
                            scoreboard.loadScores("scores.txt");
                        }
                        else if (selectedItem == 2) {
                            isOptions = true;
                            isScoreboard = false;
                        }
                        else if (selectedItem == 3) {
                            window.close();
                        }
                    }
                    else if (isOptions) {
                        optionsMenu.playSelectedMusic();
                        std::string selectedBackground = optionsMenu.getSelectedBackground();
                        if (!backgroundTexture.loadFromFile(selectedBackground)) {
                            std::cerr << "Failed to load background texture!" << std::endl;
                        }
                        backgroundSprite.setTexture(backgroundTexture);
                    }
                    else if (showRules) {
                        showRules = false;
                        isGame = true;
                        isGameRunning = true;
                        std::string selectedMap = optionsMenu.getSelectedMap();
                        game.loadMap(selectedMap);
                    }
                    else if (game.hasEnded()) {
                        game.saveScore();
                        isGameRunning = false;
                        isGame = false;
                        game.resetGame();
                        isScoreboard = true;
                        scoreboard.loadScores("scores.txt");
                    }
                }
                if (event.key.code == sf::Keyboard::Escape) {
                    if (isGameRunning) {
                        isGameRunning = false;
                        isGame = false;
                        game.resetGame();
                    }
                    else if (game.hasEnded()) {
                        isGame = false;
                        game.resetGame();
                    }
                    else if (isScoreboard || isOptions) {
                        isScoreboard = false;
                        isOptions = false;
                    }
                    else if (enterName) {
                        enterName = false;
                    }
                    else {
                        window.close();
                    }
                }
            }
        }

        if (isGameRunning) {
            sf::Vector2f dir(0.f, 0.f);
            if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up))    dir.y -= 1.f;
            if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down))  dir.y += 1.f;
            if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left))  dir.x -= 1.f;
            if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right)) dir.x += 1.f;
            game.movePlayerSmooth(dir);
        }

        float deltaTime = clock.restart().asSeconds();
        window.clear();

        if (isGameRunning) {
            game.updatePlayer(deltaTime);
            game.draw(window);
        }
        else if (game.hasEnded()) {
            game.draw(window);
        }
        else {
            window.draw(backgroundSprite);
            if (isScoreboard) {
                scoreboard.draw(window);
            }
            else if (isOptions) {
                optionsMenu.draw(window);
            }
            else if (showRules) {
                game.drawRules(window);
            }
            else if (enterName) {
                window.draw(nameText);
            }
            else {
                menu.draw(window);
            }
        }

        window.display();
    }

    return 0;
}
