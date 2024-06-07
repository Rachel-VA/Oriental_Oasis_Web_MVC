using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Oriental_Oasis_Web.Models
{
    //create a class Category and its properties
    public class Category
    {
        //create properties 
        [Key] //data annotation to allow .NET recognize the PropID is the primary key
        public int Cat_Id { get; set; }// will be the primary keys of the table for database

        [Required(ErrorMessage = "Name is required")] // Data annotation to ensure the Name property is mandatory and validation for server side
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name should be between 3 and 100 characters")]
        [DisplayName("Category Name")] // Data annotation to display the name we want on the page
        public string Name { get; set; } = null!;// The Name must have a value

		//data annotation for to display the name we want on the page and the validation for server side
		[DisplayName("Show Order")]
		[Range(1, int.MaxValue, ErrorMessage = "Order must be at least 1")]
		public int DisplayOrder { get; set; } //optional property
    }
}
