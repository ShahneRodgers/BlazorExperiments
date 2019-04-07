using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Blazor.Components;
using System.Linq;

namespace Minesweeper.Pages {
    public class MinesweeperBase : BlazorComponent {
        private int width = 9;
        private int height = 9;
        private int numBombs = 9;
        private Tile[][] tiles;

        private int numTilesHidden;
        private State state = State.Playing;

        [Parameter]
        protected int Width {
            get { return width; }
            set {
                tiles = null;
                width = value;
            }
        }

        [Parameter]
        protected int Height {
            get { return height; }
            set {
                tiles = null;
                height = value;
            }
        }

        [Parameter]
        protected int NumBombs {
            get { return numBombs; }
            set {
                tiles = null;
                numBombs = value;
            }
        }

        protected Tile[][] Tiles {
            get {
                if (tiles == null)
                    OnInit();
                return tiles;
            }
        }

        protected State CurrentState {
            get { return state; }
        }

        protected override void OnInit() {
            int remainingBombs = numBombs;
            int remainingTiles = height * width;
            Random random = new Random();
            tiles = new Tile[height][];
            for (int i = 0; i < height; ++i) {
                tiles[i] = new Tile[width];
                for (int j = 0; j < width; ++j) {
                    bool isBomb = (random.Next(remainingTiles) < remainingBombs);
                    if (isBomb)
                        remainingBombs--;

                    tiles[i][j] = new Tile(isBomb, i, j, this);

                    remainingTiles--;
                }
            }

            numTilesHidden = height * width;
        }

        private void OnTileRevealed() {
            numTilesHidden -= 1;
            if (numTilesHidden == numBombs) {
                state = State.Won;
                StateHasChanged();
            }
        }

        private void OnBombRevealed() {
            state = State.Lost;
            StateHasChanged();
        }

        protected class Tile {
            private readonly MinesweeperBase parent;

            public bool IsBomb { get; }
            public int XIndex { get; }
            public int YIndex { get; }

            public string Value { get; set; } = " ";

            public Tile(bool isBomb, int x, int y, MinesweeperBase parent) {
                IsBomb = isBomb;
                XIndex = x;
                YIndex = y;
                this.parent = parent;
            }

            public void Reveal() {
                // Already revealed
                if (Value != " ")
                    return;

                if (IsBomb) {
                    Value = "x";
                    parent.OnBombRevealed();
                    return;
                }
                
                ICollection<Tile> neighbours = GetNeighbours();

                int neighbouringBombCount = neighbours.Count(x => x.IsBomb);
                Value = neighbouringBombCount.ToString();
                parent.OnTileRevealed();
                
                // If no neighbours are bombs, reveal all.
                if (neighbouringBombCount == 0)
                    RevealNeighbours(neighbours);
            }

            private ICollection<Tile> GetNeighbours() {
                List<Tile> result = new List<Tile>();

                // For the row above, the current row and the row below
                for (int i = XIndex - 1; i <= XIndex + 1; i++) {
                    // If the index is out of bounds, continue.
                    if (i < 0 || i >= parent.Tiles.Length)
                        continue;

                    // For the column before, the current column and the next column
                    for (int j = YIndex - 1; j <= YIndex + 1; j++) {
                        // If the index is out of bounds, continue.
                        if (j < 0 || j >= parent.Tiles[i].Length)
                            continue;
                        // If this is the current node, continue.
                        if (i == XIndex && j == YIndex)
                            continue;

                        result.Add(parent.Tiles[i][j]);
                    }
                }

                return result;
            }

            private void RevealNeighbours(IEnumerable<Tile> neighbours) {
                foreach (Tile neighbour in neighbours) {
                    neighbour.Reveal();
                }
            }
        }

        protected enum State {
            Playing,
            Won,
            Lost
        }
    }
}