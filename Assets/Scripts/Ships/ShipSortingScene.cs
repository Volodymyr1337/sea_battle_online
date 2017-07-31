using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
//
// Передвижения и расстановка кораблей
//
public class ShipSortingScene : MonoBehaviour
{
    public static Transform ship = null;    // корабль в который тыцнули мышью
    List<Transform> _shipCoords = new List<Transform>();

    bool move = false;                      // контроль передвижений

    bool firstClick = true;                 // Контроль первого нажатия на кнопку авторазмещения
    float timeAutoPlaceClicked = 0;         // задержка для подстраховки корутины по авторазмещению кораблей

    public GameObject battleGroundObj;      // Полевая сетка, необходимо для 
    Battleground bg;                        // координатного размещения кораблей (Центр в [0,0])
    BoxCollider2D shipCol = null;           // Для последующей идентификации размера коллайдера
    float kX = 0f, kY = 0f;                  // коеф для поправки положения курсора и положения коробля на поле
    public static bool Landing = true;      // контроль возможности посадки корабля
    bool onTheField = false;                // нахождение корабля над игровым полем
    bool OutOfMapRange = false;             // если края корабля выходят за пределы поля
    // Коеффициенты для невозможности установить корабль за пределами поля
    float xMaxOffset, xMinOffset, yMaxOffset, yMinOffset;

    public int[,] ShipFieldPos              // Утвержденные позиции кораблей
    {
        get; private set;
    }
    public List<Ship> ShipListing = new List<Ship>();

    public Text[] shipCount;                // нумерация оставшегося кол-ва доступных кораблей

    public GameObject SetShipPosPanel;      // панель расстановки кораблей
    public GameObject BattleSceneCanvas;    // ui боевой сцены
    public GameObject EnemyField;           // Поле врага

    public Image StepArrow;                 // стрелка показывающая чей сейчас ход

    public GameObject Gun                   // Прицел выбранного орудия
    {
        get; private set;
    }
    public Text WaitingText;                // поле с текстом ожидания другого игрока

    private void Start()
    {
        bg = battleGroundObj.GetComponent<Battleground>();

        Gun = null;

        ShipFieldPos = new int[bg.size_X, bg.size_Y];

        for (int i = 0; i < bg.size_Y; i++)
            for (int j = 0; j < bg.size_X; j++)
            {
                ShipFieldPos[i, j] = 0; 
            }
                
        xMaxOffset = battleGroundObj.transform.position.x;
        xMinOffset = battleGroundObj.transform.position.x + bg.size_X;
        yMaxOffset = battleGroundObj.transform.position.y + bg.size_Y;
        yMinOffset = battleGroundObj.transform.position.y;

        for (int counter = 4, i = 1; i <= 4; i++, counter--)        // кидаем в пул кораблики
        {
            PoolManager.Instance.CreatePool(Resources.Load(i.ToString()) as GameObject, i, counter);
            shipCount[i - 1].text = PoolManager.Instance.CountOfShips(i) + "x";
        }
    }

    private void Update()
    {
        GunAimMovements();
    }

    private void LateUpdate()
    {
        ShipMovements();        
    }

    #region Ships Managmenet

    void ShipMovements()            // передвижение выбранного корабля перед установкой вручную
    {
        if (!move && ship != null && shipCol == null)
        {
            move = true;
            shipCol = ship.GetComponent<BoxCollider2D>();
            ship.GetComponentInChildren<SpriteRenderer>().sortingOrder = 2;     // выделенный корабль находится над всеми                
        }

        if (!move && ship != null && ship.tag == "Placed")
        {
            if (!Landing)       // Возврат в пул, если сработал баг при установке в уже занятую ячейку
            {
                // узнаём ключ по первому символу в имени корабля
                GameObject go = ship.gameObject;
                char[] index = go.name.ToCharArray(0, 1);
                int key = (int)Char.GetNumericValue(index[0]);

                PoolManager.Instance.ReturnShip(key, go);
                shipCount[key - 1].text = PoolManager.Instance.CountOfShips(key) + "x";   // обновляем счетчик доступных кораблей
            }
            ship.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
            ship = null;
            shipCol = null;
            Landing = true;
        }

        if (move)
        {
            Vector3 shipPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            ship.position = MovementRounder(shipPos);
            //
            // ПРОВЕРКА ПЕРЕД УСТАНОВКОЙ, ЧТО КОРАБЛЬ НЕ ВЫХОДИТ ЗА ПРЕДЕЛЫ ИГРОВОГО ПОЛЯ
            // (если позиция корабля + коллайдер/2 выходят за пределы, установка не произойдет)
            //
            if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 0 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 180)
            {
                if (onTheField && (ship.position.x - shipCol.size.x / 2) < xMaxOffset || (ship.position.x + shipCol.size.x / 2) > xMinOffset)
                    OutOfMapRange = false;
                else if (onTheField)
                    OutOfMapRange = true;
            }
            else if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 90 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 270)
            {
                if (onTheField && (ship.position.y + shipCol.size.x / 2) > yMaxOffset || (ship.position.y - shipCol.size.x / 2) < yMinOffset)
                    OutOfMapRange = false;
                else if (onTheField)
                    OutOfMapRange = true;
            }

            if (Landing && OutOfMapRange && onTheField && Input.GetMouseButtonUp(0))
            {
                ship.tag = "Placed";
                move = false;
                
                ApplyShipListing();
            }
        }        
    }

    // занесение позиций кораблей в общий список
    private void ApplyShipListing()
    {
        char[] index = ship.gameObject.name.ToCharArray(0, 1);
        int key = (int)Char.GetNumericValue(index[0]);

        int xPos = Mathf.RoundToInt(ship.position.x + 10.9f);
        int yPos = Mathf.RoundToInt(ship.position.y - 0.1f);

        if (key > 1)
        {
            if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 0 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 180)
            {
                if (key != 2)
                {
                    ShipFieldPos[Mathf.RoundToInt(xPos - shipCol.size.x / 2), yPos] = 1;
                    ShipFieldPos[xPos + 1, yPos] = 1;
                }
                if (key != 3)
                    ShipFieldPos[xPos - 1, yPos] = 1;

                ShipFieldPos[xPos, yPos] = 1;

                switch (key)
                {
                    case 2:
                        ShipListing.Add(new Ship(new ShipCoords[2] {
                                                                            new ShipCoords(xPos - 1, yPos),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    case 3:
                        ShipListing.Add(new Ship(new ShipCoords[3] {
                                                                            new ShipCoords(Mathf.RoundToInt(xPos - shipCol.size.x / 2), yPos),
                                                                            new ShipCoords(xPos + 1, yPos),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    case 4:
                        ShipListing.Add(new Ship(new ShipCoords[4] {
                                                                            new ShipCoords(Mathf.RoundToInt(xPos - shipCol.size.x / 2), yPos),
                                                                            new ShipCoords(xPos + 1, yPos),
                                                                            new ShipCoords(xPos - 1, yPos),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    default:
                        break;
                }
            }
            else if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 90 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 270)
            {
                if (key != 2)
                {
                    ShipFieldPos[xPos, Mathf.RoundToInt(yPos - shipCol.size.x / 2)] = 1;
                    ShipFieldPos[xPos, yPos + 1] = 1;
                }
                if (key != 3)
                    ShipFieldPos[xPos, yPos - 1] = 1;

                ShipFieldPos[xPos, yPos] = 1;

                switch (key)
                {
                    case 2:
                        ShipListing.Add(new Ship(new ShipCoords[2] {
                                                                            new ShipCoords(xPos, yPos - 1),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    case 3:
                        ShipListing.Add(new Ship(new ShipCoords[3] {
                                                                            new ShipCoords(xPos, Mathf.RoundToInt(yPos - shipCol.size.x / 2)),
                                                                            new ShipCoords(xPos, yPos + 1),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    case 4:
                        ShipListing.Add(new Ship(new ShipCoords[4] {
                                                                            new ShipCoords(xPos, Mathf.RoundToInt(yPos - shipCol.size.x / 2)),
                                                                            new ShipCoords(xPos, yPos - 1),
                                                                            new ShipCoords(xPos, yPos + 1),
                                                                            new ShipCoords(xPos, yPos)
                                                                            }, true, key));
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            ShipFieldPos[xPos, yPos] = 1;

            // добавляем кораблик в общий список
            ShipListing.Add(new Ship(new ShipCoords[1] { new ShipCoords(xPos, yPos) }, true, key));
        }
    }

    // Округляем перемещения с учётом угла поворота корабля, 
    // чтобы корабль можно было установить строго по сетке
    Vector3 MovementRounder(Vector3 shipPos)
    {
        if (shipPos.x > xMaxOffset && shipPos.x < xMinOffset && shipPos.y < yMaxOffset && shipPos.y > yMinOffset)
            onTheField = true;
        else
            onTheField = false;

        if (onTheField && Mathf.Abs(Mathf.Round(ship.eulerAngles.z)) != 90 && Mathf.Abs(Mathf.Round(ship.eulerAngles.z)) != 270)
        {
            kY = (shipPos.y - Mathf.Round(shipPos.y) > 0) ? 0.5f : -0.5f;

            if ((Mathf.RoundToInt(shipCol.size.x) & 1) == 1)
            {
                kX = (shipPos.x - Mathf.Round(shipPos.x) > 0) ? 0.5f : -0.5f;
            }
            else
                kX = 0f;

            return new Vector2(Mathf.Round(shipPos.x) + kX, Mathf.Round(shipPos.y) + kY);
        }
        else if (onTheField)
        {
            if ((Mathf.RoundToInt(shipCol.size.x) & 1) == 1)
            {
                kY = (shipPos.y - Mathf.Round(shipPos.y) > 0) ? 0.5f : -0.5f;
            }
            else
                kY = 0;

            kX = (shipPos.x - Mathf.Round(shipPos.x) > 0) ? 0.5f : -0.5f;

            return new Vector2(Mathf.Round(shipPos.x) + kX, Mathf.Round(shipPos.y) + kY);
        }
        else
            return new Vector2(shipPos.x, shipPos.y);   // плавное передвижение, если не над сеткой
    }
    //
    // Кнопка выбора корабля для расстановки
    //
    public void ChooseShipBtn(int Key)          
    {
        if (ship == null)
        {
            GameObject go = PoolManager.Instance.GetShip(Key);
            if (go != null)
            {
                go.SetActive(true);
                ship = go.GetComponent<Transform>();
                _shipCoords.Add(ship);
            }
            shipCount[Key - 1].text = PoolManager.Instance.CountOfShips(Key) + "x";
        }
    }
    //
    // Кнопка поворота корабля под 90
    //
    public void RotateBtn()
    {
        if (ship != null)
        {
            ship.transform.Rotate(0, 0, 90);
        }
    }
    //
    // кнопка авторазмещения корабликов
    //
    public void AutoPlacement()         
    {
        // задержка на случай, если корутина медленно выполняется
        if ((Time.time - timeAutoPlaceClicked) > .3f)       
        {
            timeAutoPlaceClicked = Time.time;

            if (ship == null)
            {
                if (firstClick)
                {
                    firstClick = false;
                    StartCoroutine(ShipPlacing());
                }
                else
                    StartCoroutine(ShipReplacing());
            }
        }
    }
    //
    // Кнопка возврата кораблей в пул и исходное положение
    //
    public void ResetButton()           
    {
        if (ship == null)
        {
            firstClick = true;
            StartCoroutine(ResetShipPositions());
        }
    }

    IEnumerator ResetShipPositions()
    {
        foreach (Transform tr in _shipCoords)
        {
            tr.tag = "Ship";
            char[] index = tr.name.ToCharArray(0, 1);
            int key = (int)Char.GetNumericValue(index[0]);
            tr.position = Vector3.zero;
            PoolManager.Instance.ReturnShip(key, tr.gameObject);            
            // обновляем счетчик доступных кораблей
            shipCount[key - 1].text = PoolManager.Instance.CountOfShips(key) + "x";

            yield return null;
        }
        _shipCoords.Clear();
        ship = null;

        ShipListing.Clear();                // очищаем список позиций кораблей
        for (int j = 0; j < bg.size_Y; j++)
            for (int i = 0; i < bg.size_X; i++)
                ShipFieldPos[i, j] = 0;  
    }

    IEnumerator ShipReplacing()
    {
        ShipListing.Clear();
        for (int j = 0; j < bg.size_Y; j++)
            for (int i = 0; i < bg.size_X; i++)
                ShipFieldPos[i, j] = 0;

        foreach (Transform tr in _shipCoords)
        {
            tr.tag = "Ship";
            tr.gameObject.SetActive(false); // чтоб не возникало столкновений при переустановке позиций
        }

        yield return null;

        foreach (Transform tr in _shipCoords)
        {
            tr.gameObject.SetActive(true);
            ship = null;
            shipCol = null;
            ship = tr;
            shipCol = tr.GetComponent<BoxCollider2D>();
            ShipAutoPositionSetter();
            yield return null;

            ApplyShipListing();         // добавляем в список позиции кораблей

            ship.tag = "Placed";
        }

    }

    IEnumerator ShipPlacing()
    {
        for (int i = 1; i < 5; i++)
        {
            while (PoolManager.Instance.CountOfShips(i) > 0)
            {
                ChooseShipBtn(i);
                shipCol = ship.GetComponent<BoxCollider2D>();
                
                ShipAutoPositionSetter();
                
                yield return null;

                ApplyShipListing();         // добавляем в список позиции кораблей

                ship.tag = "Placed";
                ship.GetComponentInChildren<SpriteRenderer>().sortingOrder = 1;
                ship = null;
                shipCol = null;
            }
        }
    }

    void ShipAutoPositionSetter()   // рекурсивно устанавливает корабли в пределах поля
    {
        ship.Rotate(0, 0, UnityEngine.Random.Range(0, 2) * 90);
        ship.position = MovementRounder(new Vector3(UnityEngine.Random.Range(xMinOffset, xMaxOffset), UnityEngine.Random.Range(yMinOffset, yMaxOffset)));

        Collider2D[] shipCollisions = Physics2D.OverlapBoxAll(ship.position, shipCol.size, ship.eulerAngles.z);
        if (shipCollisions.Length > 0)
        {
            foreach (Collider2D col in shipCollisions)
            {
                if (col.tag == "Placed")
                {
                    ShipAutoPositionSetter();
                }
            }
        }

        // Убеждаемся, что корабль не выходит за пределы поля 
        if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 0 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 180)
        {
            if ((ship.position.x - shipCol.size.x / 2) < xMaxOffset || (ship.position.x + shipCol.size.x / 2) > xMinOffset)
            {
                ShipAutoPositionSetter();
            }
        }
        else if (Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 90 || Mathf.Abs(Mathf.RoundToInt(ship.eulerAngles.z)) == 270)
        {
            if ((ship.position.y + shipCol.size.x / 2) > yMaxOffset || (ship.position.y - shipCol.size.x / 2) < yMinOffset)
            {
                ShipAutoPositionSetter();
            }
        }
    }

    #endregion


    private void GunAimMovements()   // передвижения прицела
    {
        if (Gun != null)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Если курсор не находится над полем врага ничего не произойдет
            if (pos.x < EnemyField.transform.position.x || pos.x > (EnemyField.transform.position.x + bg.size_X) ||
                pos.y < EnemyField.transform.position.y || pos.y > (EnemyField.transform.position.y + bg.size_Y))
            {
                Gun.transform.position = pos;
                return;
            }
            else
            {
                Gun.transform.position = GunAimMovementRounder(pos);

                if (Input.GetMouseButtonUp(0))
                {
                    PoolManager.Instance.ReturnGun((int)Char.GetNumericValue(Gun.name.ToCharArray(3, 1)[0]), Gun);
                    Gun = null;
                }
            }
        }
    }

    private Vector2 GunAimMovementRounder(Vector2 gunPosition)
    {
        float kx = 0, ky = 0;

        if (((int)Gun.transform.localScale.y & 1) == 1)
            ky = (gunPosition.y - Mathf.Round(gunPosition.y) > 0) ? 0.5f : -0.5f;
        else
            ky = 0;
        if (((int)Gun.transform.localScale.x & 1) == 1)
            kx = (gunPosition.x - Mathf.Round(gunPosition.x) > 0) ? 0.5f : -0.5f;
        else
            kx = 0f;

        return new Vector2(Mathf.Round(gunPosition.x) + kx, Mathf.Round(gunPosition.y) + ky);
    }

    //
    // Кнопка далее
    //
    public void NextButton()        
    {
        if (ShipListing.Count < bg.size_X)     // если не все корабли на местах
            return;
        
        InitializeUser.isReady = true;
        InitializeUser.ships = ShipFieldPos;

        WaitingText.text = "Waiting another player.";

        // Создаём объекты прицелов
        for (int i = 1; i < 7; i++)
            PoolManager.Instance.CreateWeapons(Resources.Load("Weapon/gun" + i) as GameObject, i);

        //Instantiate(Resources.Load("singleUser"));
    }
    //
    // Кнопка смены оружия
    //
    public void OnChangeWeaponParams(int area)
    {
        PlayerNetwork.Instance.RapidFire(area);
    }
    public void OnChangeWeaponBtnId(int id)
    {
        if (Gun != null)
            PoolManager.Instance.ReturnGun((int)Char.GetNumericValue(Gun.name.ToCharArray(3, 1)[0]), Gun);

        Gun = PoolManager.Instance.GetGun(id);
        Gun.SetActive(true);
    }
    //
    // кнопка покинуть лобби
    //
    public void OnClickLeaveGame()
    {
        PhotonNetwork.LeaveRoom();
               
        PhotonNetwork.LoadLevel(1);
    }

}
public class Ship       // клас содержащий информацию о корабле
{
    private bool _isAlive;          // живой ли ещё кораблик
    public bool IsAlive
    {
        get { return _isAlive; }
        set { _isAlive = value; }
    }

    private int _shootsRemaining;   // Осталось выстрелов до потопления
    public int ShootsRemaining
    {
        get { return _shootsRemaining; }
        set { _shootsRemaining = value; }
    }

    public ShipCoords[] Coords      // позиции который занимает кораблик
    {
        get; private set;
    }

    public Ship(ShipCoords[] coords, bool isAlive, int shootsRemaining)
    {
        Coords = coords;
        IsAlive = isAlive;
        ShootsRemaining = shootsRemaining;
    }
}

public struct ShipCoords
{
    public int x, y;

    public ShipCoords(int posX, int posY)
    {
        x = posX;
        y = posY;
    }

    public override string ToString()
    {
        return ("("+ x + ", " + y + ")");
    }
}

public struct ShootingArea      // описывает квадрат поражения выбранного типа оружия
{
    public float sizeX, sizeY;

    public ShootingArea(float xs = 1, float ys = 1)
    {
        sizeX = xs;
        sizeY = ys;
    }

    public override string ToString()
    {
        return ("Shooting area size (" + sizeX + ", " + sizeY + ")");
    }
}