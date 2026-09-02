import 'package:flutter/material.dart';
import '../models/customer.dart';
import '../services/customer_service.dart';
import 'customer_form_screen.dart';
import 'customer_details_screen.dart';

class CustomerListScreen extends StatefulWidget {
  const CustomerListScreen({Key? key}) : super(key: key);

  @override
  _CustomerListScreenState createState() => _CustomerListScreenState();
}

class _CustomerListScreenState extends State<CustomerListScreen> {
  final CustomerService _customerService = CustomerService();
  final TextEditingController _searchController = TextEditingController();
  
  List<Customer> _customers = [];
  bool _isLoading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadCustomers();
  }

  Future<void> _loadCustomers([String query = '']) async {
    setState(() {
      _isLoading = true;
      _error = null;
    });
    try {
      final customers = query.isEmpty 
          ? await _customerService.getCustomers() 
          : await _customerService.searchCustomers(query);
      setState(() {
        _customers = customers;
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

  void _navigateToForm({Customer? customer}) async {
    final result = await Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => CustomerFormScreen(customer: customer),
      ),
    );
    if (result == true) {
      _loadCustomers(_searchController.text);
    }
  }

  void _navigateToDetails(Customer customer) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (context) => CustomerDetailsScreen(customer: customer),
      ),
    );
  }

  Future<void> _toggleStatus(Customer customer) async {
    try {
      await _customerService.toggleStatus(customer.id, !customer.isActive);
      _loadCustomers(_searchController.text);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Status updated for ${customer.name}')),
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
        title: const Text('Customers'),
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                labelText: 'Search Customers',
                prefixIcon: const Icon(Icons.search),
                suffixIcon: IconButton(
                  icon: const Icon(Icons.clear),
                  onPressed: () {
                    _searchController.clear();
                    _loadCustomers();
                  },
                ),
                border: const OutlineInputBorder(),
              ),
              onSubmitted: (value) => _loadCustomers(value),
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
              onPressed: () => _loadCustomers(_searchController.text),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }
    if (_customers.isEmpty) {
      return const Center(child: Text('No customers found.'));
    }

    return RefreshIndicator(
      onRefresh: () => _loadCustomers(_searchController.text),
      child: ListView.builder(
        itemCount: _customers.length,
        itemBuilder: (context, index) {
          final customer = _customers[index];
          return Card(
            margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            child: ListTile(
              title: Text(
                customer.name,
                style: TextStyle(
                  decoration: customer.isActive ? TextDecoration.none : TextDecoration.lineThrough,
                  color: customer.isActive ? Colors.black : Colors.grey,
                ),
              ),
              subtitle: Text('Bal: \$${customer.currentBalance.toStringAsFixed(2)} | Mobile: ${customer.mobileNumber ?? "N/A"}'),
              trailing: PopupMenuButton<String>(
                onSelected: (value) {
                  if (value == 'edit') {
                    _navigateToForm(customer: customer);
                  } else if (value == 'toggle') {
                    _toggleStatus(customer);
                  }
                },
                itemBuilder: (context) => [
                  const PopupMenuItem(
                    value: 'edit',
                    child: Text('Edit'),
                  ),
                  PopupMenuItem(
                    value: 'toggle',
                    child: Text(customer.isActive ? 'Deactivate' : 'Activate'),
                  ),
                ],
              ),
              onTap: () => _navigateToDetails(customer),
            ),
          );
        },
      ),
    );
  }
}
