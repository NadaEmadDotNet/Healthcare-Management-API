UPDATE AspNetUsers
SET IsActive = 1, IsVisible = 1
WHERE IsActive = 0 OR IsVisible = 0;
