jQuery(document).ready(function ($) {
    /* Add active class for header navigation */
    const urlPath = window.location.pathname;
    const navElement = "nav.navbar .navbar-collapse .navbar-nav a.nav-link";
    $(navElement).each(function (index, element) {
        if (!urlPath.includes($(element).data("header-name"))) {
            if ($(element).data("header-name") === "Home") {
                $(element).addClass("active");
            }
        } else {
            $(navElement).removeClass("active");
            $(element).addClass("active");
        }
    });
});