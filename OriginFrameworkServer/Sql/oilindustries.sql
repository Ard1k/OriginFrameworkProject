-- 18.5.2021
insert into `jobs` (`name`, `label`, `whitelisted`) values ('oilindustries', 'Oil Industries', 1);

insert into `job_grades` (`job_name`, `grade`, `name`, `label`, `salary`, `skin_male`, `skin_female`) values 
('oilindustries', 0, 'employee', 'Zamestnanec', 0, '{}', '{}'),
('oilindustries', 1, 'seller', 'Prodejce', 0, '{}', '{}'), 
('oilindustries', 2, 'viceboss', 'Viceboss', 0, '{}', '{}'), 
('oilindustries', 3, 'boss', 'Boss', 0, '{}', '{}');