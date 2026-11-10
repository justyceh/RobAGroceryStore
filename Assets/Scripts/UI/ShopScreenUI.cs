using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The Shop panel on the main menu: shows your money, your hotbar and backpack (reusing
// InventoryGridUI), lets you sell the selected stack, and lists tools to buy.
// It only handles clicks and text; the actual rules live in Shop.cs.
// How to sell: click a slot to select it (it turns blue), then click Sell 1 or Sell All.
public class ShopScreenUI : MonoBehaviour
{
    [Header("Money and messages")]
    [Tooltip("Text showing how much money you have, e.g. '$120'.")]
    [SerializeField] private TMP_Text moneyText;

    [Tooltip("Text describing the selected slot, e.g. 'Pringles x3 - $10 each'.")]
    [SerializeField] private TMP_Text selectedText;

    [Tooltip("Text for feedback after a sale or purchase, e.g. 'Sold 3x Pringles for $30.'")]
    [SerializeField] private TMP_Text messageText;

    [Header("Your items")]
    [Tooltip("The InventoryGridUI that shows the hotbar inside the shop.")]
    [SerializeField] private InventoryGridUI hotbarGrid;

    [Tooltip("The InventoryGridUI that shows the backpack inside the shop.")]
    [SerializeField] private InventoryGridUI backpackGrid;

    [Header("Selling")]
    [Tooltip("Sells one item from the selected stack.")]
    [SerializeField] private Button sellOneButton;

    [Tooltip("Sells the whole selected stack.")]
    [SerializeField] private Button sellAllButton;

    [Header("Buying")]
    [Tooltip("Every item the shop sells. Each one gets a Buy button using its Buy Price.")]
    [SerializeField] private List<ItemData> itemsForSale = new List<ItemData>();

    [Tooltip("A Button prefab with a text label inside, copied once per item for sale (Assets/Prefabs/ShopBuyButton).")]
    [SerializeField] private Button buyButtonPrefab;

    [Tooltip("The object the Buy buttons are created inside. Its Vertical Layout Group stacks them.")]
    [SerializeField] private Transform buyListContainer;

    // The slot selected for selling. null = nothing selected.
    private InventoryGridUI selectedGrid;
    private int selectedIndex = -1;

    private bool isSetUp;

    // OnEnable runs every time the panel is shown (SetActive(true)), like a "visible" effect in React.
    // The panel starts hidden, so this also runs before Start the first time; setup happens here
    // instead of in Start so the grids exist before we try to select one for controller navigation.
    private void OnEnable()
    {
        if (!isSetUp)
        {
            SetUp();
        }

        ClearSelection();
        messageText.text = "";
        RefreshMoney();

        if (isSetUp && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(hotbarGrid.FirstSlotObject);
        }
    }

    // Runs once: builds the grids and Buy buttons and hooks up the Sell buttons.
    private void SetUp()
    {
        GameSession session = GameSession.Instance;
        if (session == null)
        {
            Debug.LogError("ShopScreenUI: no GameSession in the scene, so there are no items to show.");
            return;
        }

        hotbarGrid.Bind(session.Hotbar);
        backpackGrid.Bind(session.Backpack);
        hotbarGrid.SlotClicked += OnSlotClicked;
        backpackGrid.SlotClicked += OnSlotClicked;

        sellOneButton.onClick.AddListener(() => SellSelected(false));
        sellAllButton.onClick.AddListener(() => SellSelected(true));

        foreach (ItemData item in itemsForSale)
        {
            if (item == null)
            {
                continue; // An empty slot left in the Items For Sale list.
            }

            Button button = Instantiate(buyButtonPrefab, buyListContainer);
            // GetComponentInChildren is fine here: it runs once per button at setup, not every frame.
            button.GetComponentInChildren<TMP_Text>().text = $"Buy {item.DisplayName}  ${item.BuyPrice}";

            // Each button's callback remembers its own item (C# gives each loop pass a fresh "item").
            button.onClick.AddListener(() => Buy(item));
        }

        isSetUp = true;
    }

    private void OnSlotClicked(InventoryGridUI grid, int index)
    {
        // Clicking the selected slot again, or an empty slot, just deselects.
        bool sameSlot = grid == selectedGrid && index == selectedIndex;
        if (sameSlot || grid.Inventory.GetSlot(index).IsEmpty)
        {
            ClearSelection();
            return;
        }

        ClearSelection();
        selectedGrid = grid;
        selectedIndex = index;
        grid.SetPicked(index);
        ShowSelectedInfo();
    }

    private void SellSelected(bool wholeStack)
    {
        if (selectedGrid == null)
        {
            messageText.text = "Click an item in your hotbar or backpack first.";
            return;
        }

        Shop.TrySell(selectedGrid.Inventory, selectedIndex, wholeStack, out string message);
        messageText.text = message;
        RefreshMoney();

        // Selling the last one empties the slot, so there's nothing left to keep selected.
        if (selectedGrid.Inventory.GetSlot(selectedIndex).IsEmpty)
        {
            ClearSelection();
        }
        else
        {
            ShowSelectedInfo();
        }
    }

    private void Buy(ItemData item)
    {
        Shop.TryBuy(item, out string message);
        messageText.text = message;
        RefreshMoney();

        // A bought item can stack onto the selected slot, so its count may have changed.
        if (selectedGrid != null)
        {
            ShowSelectedInfo();
        }
    }

    private void ShowSelectedInfo()
    {
        InventorySlot slot = selectedGrid.Inventory.GetSlot(selectedIndex);
        string where = selectedGrid == hotbarGrid ? "Hotbar" : "Backpack";
        selectedText.text = $"{where}: {slot.Item.DisplayName} x{slot.Count}  -  ${slot.Item.SellValue} each";

        // interactable = true/false is like removing/adding the "disabled" attribute on an HTML button.
        sellOneButton.interactable = true;
        sellAllButton.interactable = true;
    }

    private void ClearSelection()
    {
        if (selectedGrid != null)
        {
            selectedGrid.SetPicked(-1);
        }
        selectedGrid = null;
        selectedIndex = -1;

        selectedText.text = "Click an item to sell it.";
        sellOneButton.interactable = false;
        sellAllButton.interactable = false;
    }

    private void RefreshMoney()
    {
        if (GameSession.Instance != null)
        {
            moneyText.text = $"${GameSession.Instance.Money}";
        }
    }
}
