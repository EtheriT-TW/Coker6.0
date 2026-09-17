import { createRouter, createWebHistory } from "vue-router";

export const router = createRouter({
    history: createWebHistory("/"),
    routes: [
        {
            path: "/",
            name: "dashboard",
            component: () => import("@/views/DashboardView.vue"),
            meta: { title: "總覽" }
        },
        {
            path: "/companies",
            name: "companies",
            component: () => import("@/views/CompaniesView.vue"),
            meta: { title: "客戶資料" }
        },
        {
            path: "/companies/new",
            name: "company-create",
            component: () => import("@/views/CompanyEditView.vue"),
            meta: { title: "新增客戶" }
        },
        {
            path: "/companies/:id(\\d+)",
            name: "company-edit",
            component: () => import("@/views/CompanyEditView.vue"),
            meta: { title: "編輯客戶" }
        },
        {
            path: "/websites",
            name: "websites",
            component: () => import("@/views/WebsitesView.vue"),
            meta: { title: "網站與站台" }
        }, {
            path: "/websites/new",
            name: "website-create",
            component: () => import("@/views/WebsiteEditView.vue"),
            meta: { title: "新增網站" }
        },
        {
            path: "/websites/:id(\\d+)",
            name: "website-edit",
            component: () => import("@/views/WebsiteEditView.vue"),
            meta: { title: "編輯網站" }
        },
        {
            path: "/licenses",
            name: "licenses",
            component: () => import("@/views/LicensesView.vue"),
            meta: { title: "版本與授權" }
        },
        {
            path: "/analytics",
            name: "analytics",
            component: () => import("@/views/AnalyticsView.vue"),
            meta: { title: "營運分析" }
        },
        {
            path: "/settings",
            name: "settings",
            component: () => import("@/views/SystemSettingsView.vue"),
            meta: { title: "系統設定" }
        },
        {
            path: "/:pathMatch(.*)*",
            name: "not-found",
            component: () => import("@/views/NotFoundView.vue"),
            meta: { title: "找不到頁面" }
        }
    ]
});

router.afterEach((to) => {
    const title = typeof to.meta.title === "string" ? to.meta.title : "Platform";
    document.title = `${title} - Coker Platform`;
});
