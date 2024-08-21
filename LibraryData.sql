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

INSERT INTO Books (Title, Author, CategoryId, Quantity)
VALUES
    ('The Great Gatsby', 'F. Scott Fitzgerald', 1, 10),
    ('1984', 'George Orwell', 2, 10),
    ('To Kill a Mockingbird', 'Harper Lee', 3, 10),
    ('The Catcher in the Rye', 'J.D. Salinger', 4, 10),
    ('Pride and Prejudice', 'Jane Austen', 5, 10),
    ('The Hobbit', 'J.R.R. Tolkien', 6, 10),
    ('Moby Dick', 'Herman Melville', 7, 10),
    ('War and Peace', 'Leo Tolstoy', 8, 10),
    ('The Odyssey', 'Homer', 9, 10),
    ('Crime and Punishment', 'Fyodor Dostoevsky', 10, 10);

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