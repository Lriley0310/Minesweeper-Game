using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;


namespace minesweeper
{
    class Box
    {
        public Vector2f position;
        public bool opened;
        public bool bomb;
        public int totalMines;
        public bool flagged;

        // ui
        public RectangleShape uiBox;
        Texture texture;
        public int type;
        public IntRect rect;

        
        public Box(Vector2f position, bool bomb, Texture texture)
        {
            this.position = position;
            this.bomb = bomb;

            this.uiBox = new RectangleShape(new Vector2f(16, 16));
            this.uiBox.Texture = texture;
            this.uiBox.Position = this.position;
            this.uiBox.TextureRect = new IntRect(type,0,10,10);

            this.rect = new IntRect(160 + (int)this.position.X, 80 + (int)this.position.Y, 16, 16);


        }
    }

    class Game
    {
        Sprite board;
         Texture texture;

        Dictionary<Vector2f, Box> boxes;

        RenderTexture renderTexture = new RenderTexture(320, 320);

        public bool LeftClick {get; set;}
        public bool RightClick {get; set;}

        public bool NotFirst {get; set;}
        public bool EnabledClick {get; set;}

        int totalFlags = 0;
        int flagLimit = 80;
        
        Sprite gameOver;
        Sprite youWon;

        bool won;
        bool lost;

        public Game()
        {
            board = new Sprite(renderTexture.Texture);
            board.Position = new Vector2f(640/2 - 320/2, 480/2 - 320/2);

            texture = new Texture("Images/tileset.png");

            boxes = new Dictionary<Vector2f, Box>();

            EnabledClick = true;

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    boxes[new Vector2f(x, y)] = new Box(new Vector2f(x * 16, y * 16), false, texture);
                }
            }

            gameOver = new Sprite(new Texture("Images/game-over.png"));
            youWon = new Sprite(new Texture("Images/you-won.png"));

            gameOver.Position = new Vector2f(0, 160 - 25);
            youWon.Position = new Vector2f(0, 160 - 25);
        }

        public void randomMines()
        {   
            int limit = 0;
            int rowLimit = 0;
            Random random = new Random();

            for (int y = 0; y < 20; y++)
            {
                rowLimit = 0;
                for (int x = 0; x < 20; x++)
                {
                    if(!boxes[new Vector2f(x, y)].opened && rowLimit < 4 && !boxes[new Vector2f(x, y)].bomb && limit < 60)
                    {
                        if(random.Next(0,8) >= 6)
                        {
                            rowLimit++;
                            limit++;
                            boxes[new Vector2f(x, y)].bomb = true;
                            boxes[new Vector2f(x, y)].type = 0;
                        }
                    }
                }
            }
        }

        public void CalculateNumbers()
        {
            Vector2f[] offset = 
            {
                new Vector2f(-1, -1),
                new Vector2f(0, -1),
                new Vector2f(1, -1),
                new Vector2f(-1, 0),
                new Vector2f(1, 0),
                new Vector2f(-1, 1),
                new Vector2f(0, 1),
                new Vector2f(1, 1)
            };
            int totalMineser = 0;

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    totalMineser = 0;

                    if(!boxes[new Vector2f(x, y)].bomb)
                    {
                        int i = 0;
                        while(i < 8)
                        {
                            if(boxes.ContainsKey(new Vector2f(x + offset[i].X, y + offset[i].Y)))
                            {
                                if(boxes[new Vector2f(x + offset[i].X, y + offset[i].Y)].bomb)
                                {
                                    totalMineser++;
                                }
                            }
                            i++;
                            // boxes[new Vector2f(x, y)].uiBox.TextureRect = new IntRect(boxes[new Vector2f(x, y)].type * 10, 0, 10, 10);
                        }
                        if(totalMineser == 0)
                        {
                            boxes[new Vector2f(x, y)].type = 9;
                        }
                        else
                        {
                            boxes[new Vector2f(x, y)].type = totalMineser;
                        }
                    }
                }
            }
        }

        private void showOthers(Vector2i tilePos)
        {
            if(tilePos.X >= 0 && tilePos.X < 20 && tilePos.Y >= 0 && tilePos.Y < 20)
            {
                
                Queue<Vector2i> queue = new Queue<Vector2i>();
                
                Dictionary<Vector2i, bool> registry = new Dictionary<Vector2i, bool>();

                // 2
                queue.Enqueue(tilePos);
                
                registry[new Vector2i(tilePos.X, tilePos.Y)] = true;

                List<int[]> dirs = new List<int[]>()
                {
                    new int[] {1, 0},
                    new int[] {-1, 0},
                    new int[] {0, 1},
                    new int[] {0, -1},
                    new int[] {1,1},
                    new int[] {1,-1},
                    new int[] {-1,1},
                    new int[] {-1,-1}
                };
                
                while(queue.Count != 0)
                {
                    Vector2i currentTile = queue.Dequeue();

                    foreach (var dir in dirs)
                    {
                        int xx = currentTile.X + dir[0];
                        int yy = currentTile.Y + dir[1];

                        if(registry.ContainsKey(new Vector2i(xx, yy)))
                        {
                            continue;
                        }
                        if(xx >= 0 && xx < 20 && yy >= 0 && yy < 20)
                        {
                            if(
                                !boxes[new Vector2f(xx, yy)].opened && boxes[new Vector2f(xx, yy)].type >= 1 && 
                                boxes[new Vector2f(xx, yy)].type < 10 && !boxes[new Vector2f(xx, yy)].bomb
                            )
                            {   
                                boxes[new Vector2f(xx, yy)].uiBox.TextureRect = new IntRect(boxes[new Vector2f(xx, yy)].type * 10, 0, 10, 10);
                                boxes[new Vector2f(xx, yy)].opened = true;
                                registry[new Vector2i(xx, yy)] = true;
                            }
                        }
                        if(xx < 0 || xx >= 20 || yy < 0 || yy >= 20)
                        {
                            continue;
                        }
                        
                        registry[new Vector2i(xx, yy)] = true;

                        if(boxes[new Vector2f(xx, yy)].type == 9)
                            queue.Enqueue(new Vector2i(xx, yy));
                    }
                }
            }
        }

        public void ShowAllMines()
        {
            foreach (var box in boxes)
            {
                if(box.Value.bomb)
                {
                    box.Value.uiBox.TextureRect = new IntRect(10 * 10, 0, 10, 10);
                }
            }
        }

        public bool CheckMines()
        {
            foreach (var box in boxes)
            {
                if(box.Value.bomb && !box.Value.flagged)
                {
                    return false;
                }
            }
            return true;
        }

        public void Update(Vector2i mouseCoords)
        {
            if(EnabledClick)
            {
                foreach (var box in boxes)
                {
                    if(LeftClick && box.Value.rect.Contains((int)mouseCoords.X, (int)mouseCoords.Y) && !box.Value.opened)
                    {
                        LeftClick = false;

                        // change view of the clicked box
                        box.Value.opened = true;

                        // if it is mine then show game over text on the screen.
                        if(box.Value.bomb)
                        {
                            ShowAllMines();
                            lost = true;
                            box.Value.type = 10;
                            EnabledClick = false;
                        }

                        if(!NotFirst)
                        {
                            randomMines();
                            CalculateNumbers();
                            NotFirst = true;
                        }
                        box.Value.uiBox.TextureRect = new IntRect(box.Value.type * 10,0,10,10);
                        
                        if(box.Value.type == 9)
                        {
                            showOthers(new Vector2i((int)box.Value.position.X/16, (int)box.Value.position.Y/16));
                        }
                    }

                    // set flag
                    if(RightClick && box.Value.rect.Contains((int)mouseCoords.X, (int)mouseCoords.Y) && totalFlags < flagLimit)
                    {
                        RightClick = false;

                        if(!box.Value.flagged && !box.Value.opened)
                        {
                            box.Value.flagged = true;
                            box.Value.uiBox.TextureRect = new IntRect(11 * 10,0,10,10);
                            totalFlags++;
                        }
                        else if(box.Value.flagged && !box.Value.opened)
                        {
                            box.Value.flagged = false;
                            box.Value.uiBox.TextureRect = new IntRect(0 * 10,0,10,10);
                            totalFlags--;
                        }


                        if(NotFirst && CheckMines())
                        {
                            won = true;
                            EnabledClick = false;
                        }
                    }
                }
            }
        }


        public void Draw(RenderTarget window)
        {
            renderTexture.Clear();

            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    renderTexture.Draw(boxes[new Vector2f(x, y)].uiBox);
                }
            }

            if(lost)
            {
                renderTexture.Draw(gameOver);
            }
            if(won)
            {
                renderTexture.Draw(youWon);
            }

            renderTexture.Display();
            
            window.Draw(board);
        }
    }
}