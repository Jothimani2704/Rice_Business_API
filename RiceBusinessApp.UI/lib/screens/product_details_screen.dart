import 'package:flutter/material.dart';
import '../models/product.dart';
import 'product_form_screen.dart';

class ProductDetailsScreen extends StatelessWidget {
  final Product product;

  const ProductDetailsScreen({Key? key, required this.product}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Product Details'),
        actions: [
          IconButton(
            icon: const Icon(Icons.edit),
            onPressed: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (context) => ProductFormScreen(product: product),
                ),
              ).then((_) => Navigator.pop(context, true)); // Return and refresh list
            },
          )
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Card(
          elevation: 4,
          child: Padding(
            padding: const EdgeInsets.all(16.0),
            child: ListView(
              children: [
                _buildDetailRow('ID', product.id.toString()),
                _buildDetailRow('Brand Name', product.brandName),
                _buildDetailRow('Product Name', product.productName),
                _buildDetailRow('Bag Size', '${product.bagSize} kg'),
                _buildDetailRow('Selling Price', '\$${product.sellingPrice.toStringAsFixed(2)}'),
                _buildDetailRow('Minimum Stock Level', '${product.minimumStockLevel} bags'),
                _buildDetailRow('Current Stock', '${product.currentStock} bags'),
                _buildDetailRow('Status', product.isActive ? 'Active' : 'Inactive'),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildDetailRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8.0),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Expanded(
            flex: 2,
            child: Text(
              label,
              style: const TextStyle(fontWeight: FontWeight.bold, color: Colors.grey),
            ),
          ),
          Expanded(
            flex: 3,
            child: Text(
              value,
              style: const TextStyle(fontSize: 16),
            ),
          ),
        ],
      ),
    );
  }
}
