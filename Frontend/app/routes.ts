import { type RouteConfig, index, layout, route, prefix } from "@react-router/dev/routes";

const featurePages = (featureName: string) => {
    return "../src/features/" + featureName + "/pages/";
};

export default [
    // TODO: Index page
    // index()

    route("login", featurePages("auth") + "LoginPage.tsx"),
    route("register", featurePages("auth") + "RegisterPage.tsx"),

    layout("./layout/MainLayout.tsx", [
        ...prefix("dashboard", [
            index(featurePages("dashboard") + "DashboardIndex.tsx"),
            route("upcoming", featurePages("dashboard") + "DashboardUpcoming.tsx"),
            route("history", featurePages("dashboard") + "DashboardHistory.tsx"),
        ]),

        route("explore", featurePages("course") + "CourseExplore.tsx"),

        ...prefix("course", [
            index(featurePages("course") + "UserCourses.tsx"),
            route(":courseId", featurePages("course") + "CourseView.tsx", [
                route("builder", featurePages("course") + "CourseBuilder.tsx"),
                route("member", featurePages("course") + "CourseMember.tsx"),
                route("activity", featurePages("course") + "CourseActivity.tsx"),

                route("lesson/:lessonId", featurePages("lesson") + "LessonMain.tsx"),
                route("assess/:assessId", featurePages("assessment") + "AssessmentInfo.tsx"),
                route("assign/:assignId", featurePages("assignment") + "AssignmentMain.tsx"),
                route("judge/:judgeId", featurePages("judge") + "JudgeMain.tsx"),
            ]),
        ]),

        route("setting", featurePages("setting") + "SettingMain.tsx"),
    ]),

    layout("./layout/AssessmentLayout.tsx", [
        route("course/:courseId/assess/:assessId/start", featurePages("assessment") + "AssessmentStart.tsx"),
    ]),

    layout("./layout/AdminLayout.tsx", [
        ...prefix("admin", [
            index(featurePages("admin") + "AdminIndex.tsx"),
            route("course", featurePages("admin") + "AdminCourse.tsx"),
            route("user", featurePages("admin") + "AdminUser.tsx"),
        ]),
    ]),
] satisfies RouteConfig;
