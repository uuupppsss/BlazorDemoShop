window.authSession = {
    signIn: async function (payload) {
        // Вызывает локальный endpoint Blazor, который записывает auth-cookie текущего хоста.
        const response = await fetch("/auth/session/signin", {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const message = await response.text();
            throw new Error(message || "Не удалось сохранить сессию.");
        }
    },
    signOut: async function () {
        // Вызывает локальный endpoint Blazor, который удаляет auth-cookie.
        const response = await fetch("/auth/session/signout", {
            method: "POST",
            credentials: "include"
        });

        if (!response.ok) {
            const message = await response.text();
            throw new Error(message || "Не удалось завершить сессию.");
        }
    }
};
