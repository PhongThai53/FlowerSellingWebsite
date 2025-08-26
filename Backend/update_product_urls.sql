-- Update product URLs to use new format
-- This script will update all product URLs to point to primary.jpg in the images folder

UPDATE Products 
SET product_url = '/images/products/' + CAST(Id AS VARCHAR) + '/primary.jpg'
WHERE IsDeleted = 0;

-- Verify the update
SELECT Id, Name, product_url FROM Products WHERE IsDeleted = 0 ORDER BY Id;
