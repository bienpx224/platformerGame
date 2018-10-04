﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using platformerGame.Utilities;

namespace platformerGame.App
{
    abstract class BaseGuiItem
    {
        public delegate void clickCallBack(MouseButtonEventArgs e);

        public clickCallBack OnClick;

        protected GameState parent = null;

        public AABB Bounds { get; set; }
        public bool Active { get; set; }
        public bool MouseHover { get; set; }
        protected RenderStates rstates;

        public BaseGuiItem(GameState parent, AABB bounds)
        {
            this.parent = parent;
            this.Bounds = bounds;
            this.OnClick = null;
            this.Active = false;
            this.MouseHover = false;
            this.rstates = new RenderStates(BlendMode.Alpha);
        }

        public GameState Parent
        {
            get { return this.parent; }
        }

        public void Click(MouseButtonEventArgs e)
        {
            this.OnClick?.Invoke(e);
        }

        abstract public void Update(float step_time);
        abstract public void Render(RenderTarget destination);
    }

    class Button : BaseGuiItem
    {
        
        public string Title { get; set; }

        RectangleShape rectShape;
        Text label;
        Vector2f labelOrigPos = new Vector2f();

        const uint FONT_SIZE = 16;

        Color fillColor = Color.Transparent;
        Color textColor = Color.White;

        public Button(GameState parent, AABB bounds, string title) : base(parent, bounds)
        {
            this.Title = title;
            this.rectShape = new RectangleShape();
            rectShape.Position = Bounds.topLeft;
            rectShape.Size = Bounds.dims;
            rectShape.OutlineColor = Color.White;
            rectShape.OutlineThickness = 2.0f;
            rectShape.FillColor = fillColor;

            label = new Text(this.Title, AssetManager.GetFont("pf_tempesta_seven"), FONT_SIZE);
            labelOrigPos.X = Bounds.center.X - (float)(label.GetGlobalBounds().Width / 2.0f);
            labelOrigPos.Y = Bounds.center.Y - (float)(FONT_SIZE / 2.0f);
            label.FillColor = textColor;

            label.Position = new Vector2f(labelOrigPos.X, labelOrigPos.Y);
        }



        public override void Render(RenderTarget destination)
        {
            this.rectShape.FillColor = this.fillColor;
            this.label.FillColor = textColor;

            destination.Draw(this.rectShape, this.rstates);
            destination.Draw(this.label);
        }

        public override void Update(float step_time)
        {
            
            this.MouseHover = cCollision.IsPointInsideBox(parent.GetMousePos(), Bounds);
            if(MouseHover)
            {
                this.fillColor = Color.White;
                this.textColor = Color.Black;
            }
            else
            {
                this.fillColor = Color.Black;
                this.textColor = Color.White;
            }
        }
    }

    class MenuScreen
    {
        protected List<BaseGuiItem> guiItems;

        public MenuScreen()
        {
            this.guiItems = new List<BaseGuiItem>();
        }

        public void Add(BaseGuiItem[] items)
        {
            this.guiItems.AddRange(items);
        }

        public void Remove(BaseGuiItem item)
        {
            this.guiItems.Remove(item);
        }

        public void Update(float step_time)
        {
            foreach(var item in this.guiItems)
            {
                item.Update(step_time);
            }
        }

        public void Render(RenderTarget destination)
        {
            foreach (var item in this.guiItems)
            {
                item.Render(destination);
            }
        }

        public List<BaseGuiItem> Items
        {
            get { return this.guiItems; }
        }
    }

    class MainMenu : GameState
    {
        Dictionary<string, MenuScreen> menus;
        MenuScreen currentMenu = null;

        public MainMenu(SfmlApp app_ref) : base(app_ref)
        {
            this.menus = new Dictionary<string, MenuScreen>();
            this.menus.Add("home", new MenuScreen());
        }

        
        public void connectItems(string menu, BaseGuiItem[] items = null)
        {

            MenuScreen s;
            if (menus.TryGetValue(menu, out s))
            {
                s.Add(items);
                return;
            }

            s = new MenuScreen();
            s.Add(items);
            menus.Add(menu, s);
        }

        public void SwitchMenu(string name)
        {
            /*
            if(name == "back")
            {
                return;
            }
            */

            MenuScreen s;
            if(menus.TryGetValue(name, out s))
            {
                this.currentMenu = s;
            }
        }

        public void Create()
        {
            this.connectItems("home", new[] {
                new Button(this, new AABB(200,200,180,40), "Play") {
                               
                               OnClick = (MouseButtonEventArgs e) =>
                               {
                                   // this.SwitchMenu("options");
                                   this.appControllerRef.StartGame();
                               }
                },
                new Button(this, new AABB(200,300,180,40), "Exit") {
                               
                               OnClick = (MouseButtonEventArgs e) =>
                               {
                                   this.appControllerRef.CloseApp();
                               }
                }

            });

            /*
            this.connectItems("options", new[] {
                new Button() { Title = "ok" },
                new Button() { Title = "back" }

            });

            this.connectItems("credits", new[] {
                new Button() { Title = "ok" },
                new Button() { Title = "back" }

            });
            */
        }

        public override void Enter()
        {
            camera = new Camera(new View(new Vector2f(appControllerRef.WindowSize.X / 2.0f, appControllerRef.WindowSize.Y / 2.0f), appControllerRef.WindowSize));
            camera.Zoom = 1.0f;
            this.Create();
            this.SwitchMenu("home");
        }

        public override void Exit()
        {
            
        }

        public override void HandleKeyPress(KeyEventArgs e)
        {
            
        }

        public override void HandleSingleMouseClick(MouseButtonEventArgs e)
        {
            var buttons = currentMenu.Items.OfType<Button>();
            Vector2f mousePos = new Vector2f(e.X, e.Y);
            foreach (var button in buttons)
            {
                if (cCollision.IsPointInsideBox(mousePos, button.Bounds))
                {
                    button.Click(e);
                    return;
                }
            }
        }

        public override void UpdateFixed(float step_time)
        {
            currentMenu?.Update(step_time);
        }

        public override void UpdateVariable(float step_time = 1)
        {

        }

        public override void Render(RenderTarget destination, float alpha)
        {
            camera.DeployOn(destination);
            currentMenu?.Render(destination);
        }

       
    }
}
