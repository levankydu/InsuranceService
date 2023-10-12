function RenderSidebar() {
    document.addEventListener("DOMContentLoaded", (e) => {
        const sidebarMenu = document.querySelector(".sidebar-menu");
        if (!sidebarMenu) return;
        const sidebarItems = sidebarMenu.querySelectorAll("li.sidebar-item");
        if (!sidebarItems) return;
        sidebarItems.forEach(itm => itm.classList.remove("active"));
        const myItm = Array.from(sidebarItems)
            .find(itm =>
                itm.querySelector("span").innerText == "Life Insurance");
        if (myItm) myItm.classList.add("active");
    });
}
