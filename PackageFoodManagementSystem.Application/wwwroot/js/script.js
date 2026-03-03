/* =========================================================================
   PFMS Frontend Script (Sign Up / Sign In + Password Toggle)
   ========================================================================= */

/* --------------------------
 * Password visibility toggle
 * -------------------------- */
(function () {
    function toggle(btn) {
        const id = btn.getAttribute("data-target");
        const input = document.getElementById(id);
        if (!input) return;

        const icon = btn.querySelector("i");
        const hidden = input.type === "password";

        input.type = hidden ? "text" : "password";
        btn.setAttribute("aria-pressed", hidden ? "true" : "false");
        btn.setAttribute("aria-label", hidden ? "Hide password" : "Show password");

        if (icon) {
            icon.classList.toggle("fa-eye", !hidden);
            icon.classList.toggle("fa-eye-slash", hidden);
        }
    }

    document.addEventListener("click", function (e) {
        const btn = e.target.closest(".toggle-password");
        if (!btn) return;
        e.preventDefault();
        toggle(btn);
    });

    document.addEventListener("keydown", function (e) {
        const btn = e.target.closest(".toggle-password");
        if (!btn) return;
        if (e.key === " " || e.key === "Enter") {
            e.preventDefault();
            toggle(btn);
        }
    });
})();

/* --------------------------
 * jQuery: validations & submit
 * -------------------------- */
$(function () {
    /* ========= Sign Up ========= */
    const strongPwdRegex = /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{12,}$/;

    function showError(id, message) { $(id).text(message); }
    function clearError(id) { $(id).text(""); }

    function validateName() {
        const ok = $("#name").val().trim().length >= 2;
        ok ? clearError("#nameError") : showError("#nameError", "Enter a Valid Name.");
        return ok;
    }

    function validateMobile() {
        const ok = /^[0-9]{10}$/.test($("#mobilenumber").val().trim());
        ok ? clearError("#mobileError") : showError("#mobileError", "Enter a valid 10-digit mobile number.");
        return ok;
    }

    function validateEmail() {
        const ok = /^[^\s@]+@[^\s@]+\.com$/.test($("#email").val().trim());
        ok ? clearError("#emailError") : showError("#emailError", "Enter a valid email address.");
        return ok;
    }

    function validatePassword() {
        const pwd = $("#password").val();
        const ok = strongPwdRegex.test(pwd);
        ok ? clearError("#passwordError")
            : showError("#passwordError", "Password must be ≥12 chars, include uppercase, lowercase, number, special char.");
        return ok;
    }

    function validateConfirmPassword() {
        const pwd = $("#password").val();
        const cpwd = $("#confirmPassword").val();
        const ok = cpwd === pwd;
        ok ? clearError("#confirmPasswordError") : showError("#confirmPasswordError", "Passwords do not match.");
        return ok;
    }

    function validateSignUpAll() {
        return validateName() && validateMobile() && validateEmail() && validatePassword() && validateConfirmPassword();
    }

    $("#name").on("blur input", validateName);
    $("#mobilenumber").on("blur input", validateMobile);
    $("#email").on("blur input", validateEmail);
    $("#password").on("blur input", function () {
        validatePassword();
        validateConfirmPassword();
    });
    $("#confirmPassword").on("blur input", validateConfirmPassword);

    $("form[asp-action='SignUp']").on("submit", function (e) {
        const ok = validateSignUpAll();
        if (!ok) {
            e.preventDefault();
            alert("Please fix the highlighted fields.");
        }
    });

    /* ========= Sign In ========= */
    function validateSigninEmail() {
        const ok = /^[^\s@]+@[^\s@]+\.com$/.test($("#email").val().trim());
        ok ? clearError("#emailError") : showError("#emailError", "Enter a valid email address.");
        return ok;
    }

    $("#email").on("blur input", validateSigninEmail);

    $("form[asp-action='SignIn']").on("submit", function (e) {
        const okEmail = validateSigninEmail();
        if (!okEmail) {
            e.preventDefault();
            alert("Please fix the highlighted fields.");
        }
    });

    /* --------------------------
     * Logout confirmation
     * -------------------------- */

    // MODIFIED: We removed the direct click listener from here because 
    // it was conflicting with the enableIntercept logic below.
    // The confirmation will now be handled by the route/navigation logic.

    // Function to enable back/forward interception
    function enableIntercept() {
        const path = window.location.pathname.toLowerCase();

        const adminRoutes = [
            "/home/admindashboard",
            "/home/users",
            "/home/stores",
            "/home/admininventory",
            "/home/report"
        ];

        const authRoutes = [
            "/home/logout",
            "/home/signin"
        ];

        if (adminRoutes.includes(path)) {
            history.pushState(null, "", location.href);
            window.onpopstate = function () {
                const confirmed = confirm("Do you want to logout?");
                if (confirmed) {
                    window.location.href = "/Home/Logout";
                } else {
                    history.pushState(null, "", location.href);
                }
            };
        }

        if (authRoutes.includes(path)) {
            history.pushState(null, "", location.href);
            window.onpopstate = function () {
                const confirmed = confirm("You need to Login");
                if (confirmed) {
                    window.location.href = "/Home/SignIn";
                } else {
                    history.pushState(null, "", location.href);
                }
            };
        }
    }

    // Run once when DOM is ready
    document.addEventListener("DOMContentLoaded", enableIntercept);

    // Run again when page is restored from history
    window.addEventListener("pageshow", enableIntercept);
});