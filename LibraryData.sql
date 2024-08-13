select * from Books
select * from Categories
select * from Roles
select * from Users

INSERT INTO Categories (CategoryName)
VALUES 
    ('Classic Literature'),    -- CategoryId = 1
    ('Dystopian'),             -- CategoryId = 2
    ('Historical Fiction'),    -- CategoryId = 3
    ('Modern Classics'),       -- CategoryId = 4
    ('Romance'),               -- CategoryId = 5
    ('Fantasy'),               -- CategoryId = 6
    ('Adventure'),             -- CategoryId = 7
    ('Epic Literature'),       -- CategoryId = 8
    ('Mythology'),             -- CategoryId = 9
    ('Psychological Fiction'); -- CategoryId = 10

INSERT INTO Books (Title, Author, CategoryId)
VALUES
    ('The Great Gatsby', 'F. Scott Fitzgerald', 1),
    ('1984', 'George Orwell', 2),
    ('To Kill a Mockingbird', 'Harper Lee', 3),
    ('The Catcher in the Rye', 'J.D. Salinger', 4),
    ('Pride and Prejudice', 'Jane Austen', 5),
    ('The Hobbit', 'J.R.R. Tolkien', 6),
    ('Moby Dick', 'Herman Melville', 7),
    ('War and Peace', 'Leo Tolstoy', 8),
    ('The Odyssey', 'Homer', 9),
    ('Crime and Punishment', 'Fyodor Dostoevsky', 10);

INSERT INTO Roles (RoleName)
VALUES 
	('User'),
	('Admin')

DELETE FROM Roles
WHERE RoleId IN (
    SELECT RoleId FROM Roles
    ORDER BY RoleId DESC
    OFFSET 0 ROWS
    FETCH NEXT 2 ROWS ONLY
);