INSERT INTO identity_roles (id, name, normalized_name, concurrency_stamp)
VALUES
    ('role-administrator', 'Administrator', 'ADMINISTRATOR', 'seed-role-administrator'),
    ('role-student', 'Student', 'STUDENT', 'seed-role-student')
ON CONFLICT (id) DO NOTHING;

INSERT INTO users (
    id,
    full_name,
    email,
    password_hash,
    role,
    created_at,
    updated_at,
    user_name,
    normalized_user_name,
    normalized_email,
    email_confirmed,
    security_stamp,
    concurrency_stamp,
    phone_number_confirmed,
    two_factor_enabled,
    lockout_enabled,
    access_failed_count
)
VALUES
    (
        'user-admin',
        'Администратор StudyTracker',
        'admin@studytracker.local',
        'AQAAAAIAAYagAAAAEAECAwQFBgcICQoLDA0ODxDeKV9fFbpK4IHFMvQAgutrgfEd3m1egDS2qenH7m1ACA==',
        'Administrator',
        now(),
        now(),
        'admin@studytracker.local',
        'ADMIN@STUDYTRACKER.LOCAL',
        'ADMIN@STUDYTRACKER.LOCAL',
        TRUE,
        'seed-admin-security',
        'seed-admin-concurrency',
        FALSE,
        FALSE,
        TRUE,
        0
    ),
    (
        'user-student-anna',
        'Анна Смирнова',
        'student@studytracker.local',
        'AQAAAAIAAYagAAAAEBESExQVFhcYGRobHB0eHyDwM6BVByXtL3nM2MGl1SKCFcRHMsN4pKpPPB10IjAGRg==',
        'Student',
        now(),
        now(),
        'student@studytracker.local',
        'STUDENT@STUDYTRACKER.LOCAL',
        'STUDENT@STUDYTRACKER.LOCAL',
        TRUE,
        'seed-student-security',
        'seed-student-concurrency',
        FALSE,
        FALSE,
        TRUE,
        0
    ),
    (
        'user-student-ivan',
        'Иван Петров',
        'ivan@studytracker.local',
        'AQAAAAIAAYagAAAAECEiIyQlJicoKSorLC0uLzCiwd+Da/bvS9nNv6oLwmBGqL+JavlChZPkuPfIv2fFrw==',
        'Student',
        now(),
        now(),
        'ivan@studytracker.local',
        'IVAN@STUDYTRACKER.LOCAL',
        'IVAN@STUDYTRACKER.LOCAL',
        TRUE,
        'seed-ivan-security',
        'seed-ivan-concurrency',
        FALSE,
        FALSE,
        TRUE,
        0
    )
ON CONFLICT (id) DO NOTHING;

INSERT INTO identity_user_roles (user_id, role_id)
VALUES
    ('user-admin', 'role-administrator'),
    ('user-student-anna', 'role-student'),
    ('user-student-ivan', 'role-student')
ON CONFLICT (user_id, role_id) DO NOTHING;

INSERT INTO courses (id, name, description, professor_name, created_at, updated_at)
VALUES
    (1, 'Основы программирования', 'Базовый курс по C#, алгоритмам и структурам данных.', 'Проф. Елена Волкова', now(), now()),
    (2, 'Базы данных', 'Проектирование схем, SQL-запросы и основы транзакций.', 'Доц. Сергей Орлов', now(), now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO study_tasks (id, course_id, title, description, deadline, status, created_at, updated_at)
VALUES
    (1, 1, 'Лабораторная работа 1', 'Реализовать консольное приложение для обработки массива.', CURRENT_DATE + 5, 'NotStarted', now(), now()),
    (2, 1, 'Практика по LINQ', 'Подготовить выборки, сортировки и группировки коллекций.', CURRENT_DATE + 12, 'NotStarted', now(), now()),
    (3, 2, 'ER-диаграмма', 'Спроектировать ER-модель для учебного проекта.', CURRENT_DATE - 2, 'NotStarted', now(), now()),
    (4, 2, 'SQL-запросы', 'Написать запросы SELECT, JOIN, GROUP BY и HAVING.', CURRENT_DATE + 7, 'NotStarted', now(), now())
ON CONFLICT (id) DO NOTHING;

INSERT INTO student_courses (id, student_id, course_id, assigned_at)
VALUES
    (1, 'user-student-anna', 1, now()),
    (2, 'user-student-anna', 2, now()),
    (3, 'user-student-ivan', 2, now())
ON CONFLICT (student_id, course_id) DO NOTHING;

INSERT INTO student_task_statuses (id, student_id, task_id, status, updated_at)
VALUES
    (1, 'user-student-anna', 2, 'InProgress', now()),
    (2, 'user-student-ivan', 3, 'NotStarted', now())
ON CONFLICT (student_id, task_id) DO NOTHING;

SELECT setval(pg_get_serial_sequence('courses', 'id'), COALESCE((SELECT MAX(id) FROM courses), 1));
SELECT setval(pg_get_serial_sequence('study_tasks', 'id'), COALESCE((SELECT MAX(id) FROM study_tasks), 1));
SELECT setval(pg_get_serial_sequence('student_courses', 'id'), COALESCE((SELECT MAX(id) FROM student_courses), 1));
SELECT setval(pg_get_serial_sequence('student_task_statuses', 'id'), COALESCE((SELECT MAX(id) FROM student_task_statuses), 1));
