export module Health;

import <SFML/Graphics.hpp>;
import <iostream>;
import <vector>;

export class Health {
public:
    Health(int initialLives, float startX, float startY, int tileSize)
        : lives(initialLives), startX(startX), startY(startY), tileSize(tileSize)
    {
        loadTexture();
        for (int i = 0; i < lives; ++i) {
            sf::Sprite spr;
            spr.setTexture(texture);
            spr.setPosition(startX + i * (tileSize * scale + 5), startY);
            spr.setScale(sf::Vector2f(tileSize * scale / texture.getSize().x,
                tileSize * scale / texture.getSize().y));
            lifeIcons.push_back(spr);
        }
    }

    void loseLife() {
        if (lives > 0) {
            lives--;
            lifeIcons.pop_back();
        }
    }

    int getLives() const {
        return lives;
    }

    void draw(sf::RenderWindow& window) const {
        for (auto& icon : lifeIcons) {
            window.draw(icon);
        }
    }

    void reset(int newLives) {
        lives = newLives;
        lifeIcons.clear();
        for (int i = 0; i < lives; ++i) {
            sf::Sprite spr;
            spr.setTexture(texture);
            spr.setPosition(startX + i * (tileSize * scale + 5), startY);
            spr.setScale(sf::Vector2f(tileSize * scale / texture.getSize().x,
                tileSize * scale / texture.getSize().y));
            lifeIcons.push_back(spr);
        }
    }

    void reloadTextures() {
        if (!texture.loadFromFile("Serce.png")) {
            std::cerr << "Failed to load life texture!" << std::endl;
        }
    }

private:
    void loadTexture() {
        if (!texture.loadFromFile("Serce.png")) {
            std::cerr << "Failed to load life texture!" << std::endl;
        }
    }

    int lives;
    float startX, startY;
    int tileSize;
    float scale = 2.5f;
    sf::Texture texture;
    std::vector<sf::Sprite> lifeIcons;
};
