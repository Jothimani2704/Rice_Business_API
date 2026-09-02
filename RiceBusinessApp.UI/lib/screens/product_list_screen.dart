import 'package:flutter/material.dart';
import '../models/product.dart';
import '../services/product_service.dart';
import 'product_form_screen.dart';
import 'product_details_screen.dart';

class ProductListScreen extends StatefulWidget {
  const ProductListScreen({Key? key}) : super(key: key);

  @override
  _ProductListScreenState createState() => _ProductListScreenState();
}

class _ProductListScreenState extends State<ProductListScreen> {
  final ProductService _productService = ProductService();
  final TextEditingController _searchController = TextEditingController();
  
  List<Product> _products = [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadProducts();
  }

  Future<void> _loadProducts([String query = '']) async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final products = query.isEmpty 
          ? await _productService.getProducts() 
          : await _productService.searchProducts(query);
      setState(() {
        _products = products;
      });
    } catch (e) {
      setState(() {
        _error = e.toString();
      });
    } finally {
      setState(() {
        _isLoading = false;
      });
    }
  }

  void _navigateToForm({Product? product}) async {
    final result = await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ProductFormScreen(product: product),
      ),
    );
    if (result == true) {
      _loadProducts(_searchController.text);
    }
  }

  void _navigateToDetails(Product product) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => ProductDetailsScreen(product: product),
      ),
    );
  }

  Future<void> _toggleStatus(Product product) async {
    try {
      await _productService.toggleStatus(product.id, !product.isActive);
      _loadProducts(_searchController.text);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Status updated for ${product.productName}')),
      );
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Failed to update status: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Products'),
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                labelText: 'Search Products',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  icon: const Icon(Icons.clear),
                  onPressed: () {
                    _searchController.clear();
                    _loadProducts();
                  },
                ),
                border: const OutlineInputBorder(),
              ),
              onSubmitted: (value) => _loadProducts(value),
            ),
          ),
          Expanded(
            child: _buildBody(),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _navigateToForm(),
        child: const Icon(Icons.add),
      ),
    );
  }

  Widget _buildBody() {
    if (_isLoading) {
      return const Center(child: CircularProgressIndicator());
    }
    if (_error != null) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text('Error: $_error', style: const TextStyle(color: Colors.red)),
            const SizedBox(height: 10),
            ElevatedButton(
              onPressed: () => _loadProducts(_searchController.text),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }
    if (_products.isEmpty) {
      return const Center(child: Text('No products found.'));
    }

    return RefreshIndicator(
      onRefresh: () => _loadProducts(_searchController.text),
      child: ListView.builder(
        itemCount: _products.length,
        itemBuilder: (context, index) {
          final product = _products[index];
          return Card(
            margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            child: ListTile(
              title: Text(
                '${product.brandName} - ${product.productName}',
                style: TextStyle(
                  decoration: product.isActive ? TextDecoration.none : TextDecoration.lineThrough,
                  color: product.isActive ? Colors.black : Colors.grey,
                ),
              ),
              subtitle: Text('Price: \$${product.sellingPrice.toStringAsFixed(2)} | Stock: ${product.currentStock} bags'),
              trailing: PopupMenuButton<String>(
                onSelected: (value) {
                  if (value == 'edit') {
                    _navigateToForm(product: product);
                  } else if (value == 'toggle') {
                    _toggleStatus(product);
                  }
                },
                itemBuilder: (context) => [
                  const PopupMenuItem(
                    value: 'edit',
                    child: Text('Edit'),
                  ),
                  PopupMenuItem(
                    value: 'toggle',
                    child: Text(product.isActive ? 'Deactivate' : 'Activate'),
                  ),
                ],
              ),
              onTap: () => _navigateToDetails(product),
            ),
          );
        },
      ),
    );
  }
}
