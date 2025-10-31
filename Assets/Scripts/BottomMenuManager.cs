using UnityEngine;

public class BottomMenuManager : MonoBehaviour
{
    public BottomMenu[] bottomMenus;

    void Start()
    {
        if (bottomMenus == null || bottomMenus.Length == 0)
        {
            Debug.LogWarning("No bottom menus assigned!");
            return;
        }
        DistanciateTabButtons();
    }

    void Update()
    {
        // if number is pressed with ctrl, assign bottom menu to bottomMenus as many as the number pressed
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) GenerateMenus(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) GenerateMenus(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) GenerateMenus(3);
        }
    }

    void GenerateMenus(int count)
    {
        Debug.Log($"Setting bottom menus to {count}");

        for (int i = 0; i < count; i++)
        {
            if (i < bottomMenus.Length)
            {
                // Make sure the slot exists
                if (bottomMenus[i] == null)
                {
                    bottomMenus[i] = gameObject.AddComponent<BottomMenu>();
                }

                bottomMenus[i].indexNum = i;
            }
        }

        DistanciateTabButtons();
    }

    void DistanciateTabButtons()
    {
        for (int i = 0; i < bottomMenus.Length; i++)
        {
            BottomMenu menu = bottomMenus[i];
            menu.indexNum = i;

            if (menu.tabButton == null)
            {
                Debug.LogWarning($"Bottom menu {i} is missing a tabButton reference!");
                continue;
            }

            Vector3 newPosition;

            if (i == 0)
            {
                newPosition = menu.tabButton.anchoredPosition;
            }
            else
            {
                RectTransform prevButton = bottomMenus[i - 1].tabButton;

                float offsetX = prevButton.rect.width * 0.9f + prevButton.rect.width;

                newPosition = new Vector2(
                    prevButton.anchoredPosition.x + offsetX,
                    prevButton.anchoredPosition.y
                );
            }

            menu.tabButton.anchoredPosition = newPosition;
        }
    }

}
