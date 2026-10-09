#include <iostream>
#include <vector>
#include <cmath>
#include <map>
#include <future>
#include <algorithm>

using Clause = std::vector<int>;
using VariableAssignment = std::map<int, bool>;

struct LogicConstraintMatrix {
    int num_variables = 0;
    int num_clauses = 0;
    std::vector<Clause> clause_list;

    void PrintMatrixSummary() const {
        std::cout << "[C++ Logic Matrix Initialization Summary]\n"
                  << "  Total Registered Variables: " << num_variables << "\n"
                  << "  Total Ingested Clauses:   " << num_clauses << "\n";
    }
};

class DimacsParser {
public:
    static LogicConstraintMatrix IngestRawClauses(const std::vector<std::vector<int>>& raw_clauses) {
        LogicConstraintMatrix matrix;
        matrix.num_clauses = raw_clauses.size();
        int max_var_id = 0;

        for (const auto& raw_clause : raw_clauses) {
            Clause formatted_clause;
            for (int literal : raw_clause) {
                if (literal != 0) {
                    formatted_clause.push_back(literal);
                    int var_id = std::abs(literal);
                    if (var_id > max_var_id) max_var_id = var_id;
                }
            }
            matrix.clause_list.push_back(formatted_clause);
        }
        matrix.num_variables = max_var_id;
        return matrix;
    }
};

class ParallelVerificationHarness {
private:
    // Worker task to evaluate a discrete segment of the overall matrix footprint
    static bool VerifyBatchRange(
        const std::vector<Clause>& clauses, 
        const VariableAssignment& assignment, 
        size_t start_idx, 
        size_t end_idx
    ) {
        for (size_t i = start_idx; i < end_idx; ++i) {
            bool clause_satisfied = false;
            for (int literal : clauses[i]) {
                int var_id = std::abs(literal);
                bool is_negative = (literal < 0);

                if (assignment.find(var_id) == assignment.end()) continue;

                bool current_truth = assignment.at(var_id);
                if (is_negative ? !current_truth : current_truth) {
                    clause_satisfied = true;
                    break;
                }
            }
            if (!clause_satisfied) return false; // Immediate local clause failure
        }
        return true;
    }

public:
    static bool VerifySolutionParallel(const LogicConstraintMatrix& matrix, const VariableAssignment& assignment) {
        std::cout << "\n[Parallel Harness Triggered]: Launching concurrent verification threads...\n";
        
        unsigned int hardware_threads = std::max(1u, std::thread::hardware_concurrency());
        size_t total_clauses = matrix.clause_list.size();
        size_t batch_size = (total_clauses + hardware_threads - 1) / hardware_threads;

        std::vector<std::future<bool>> futures;

        for (unsigned int t = 0; t < hardware_threads; ++t) {
            size_t start_idx = t * batch_size;
            size_t end_idx = std::min(start_idx + batch_size, total_clauses);

            if (start_idx >= total_clauses) break;

            // Offload work chunk asynchronously onto separate thread contexts
            futures.push_back(std::async(
                std::launch::async, 
                &ParallelVerificationHarness::VerifyBatchRange, 
                std::ref(matrix.clause_list), 
                std::ref(assignment), 
                start_idx, 
                end_idx
            ));
        }

        // Aggregate worker matrix conclusions
        bool system_stable = true;
        for (auto& fut : futures) {
            if (!fut.get()) {
                system_stable = false; // A single false batch invalidates the hardware state configuration
            }
        }

        if (system_stable) {
            std::cout << "  [Parallel Verification Success]: 100% of clauses satisfied. Equilibrium established.\n";
        } else {
            std::cout << "  [Parallel Verification Failed]: Logical state configuration violates active physical constraints.\n";
        }
        return system_stable;
    }
};

int main() {
    std::cout << "--- Initializing Multi-Threaded C++ GCC Logic Engine ---\n";

    std::vector<std::vector<int>> external_json_clauses = {
        {-1, 2}, {-2, 3}, {-3, 1}, {-1, 3}
    };

    LogicConstraintMatrix dynamic_matrix = DimacsParser::IngestRawClauses(external_json_clauses);
    dynamic_matrix.PrintMatrixSummary();

    VariableAssignment assignment = {{1, true}, {2, true}, {3, true}};
    bool result = ParallelVerificationHarness::VerifySolutionParallel(dynamic_matrix, assignment);
    
    std::cout << "\nExecution Thread Ring Closed. Status: " << (result ? "STABLE" : "UNSTABLE") << "\n";
    return 0;
}
